using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using LTSystem.Core;
using System.Collections.Generic;
using UnityEngine.Events;

namespace LTSystem.Interaction
{
    using LTSystem.Core;
    using LTSystem.Network;
    using LTSystem.Animation;

    /// <summary>
    /// Placed on any GameObject that can initiate a LoveTrigger interaction.
    /// Manages the full encounter state machine:
    /// IDLE → DETECTION → PROMPTED → CONSENT_PENDING → EXECUTING → RESTORING → IDLE
    ///
    /// DEBT-D NOTE: UI setup methods are protected virtual — InteractableObjectFix.cs
    /// is no longer required and should be deleted.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────
        [Header("Triggers")]
        [Tooltip("All LoveTriggers available at this interactable.")]
        [SerializeField] private List<LoveTriggerSO> _availableTriggers = new List<LoveTriggerSO>();

        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 2f;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Timing")]
        [SerializeField] private float _consentTimeoutSeconds = 15f;

        [Header("References")]
        [SerializeField] private UniversalAnimationController _animationController;
        [SerializeField] private InteractableTimelineController _timelineController;

        [Header("Events")]
        public UnityEvent<LoveTriggerRequest> OnTriggerExecuted;
        public UnityEvent<LoveTriggerRequest> OnTriggerDenied;
        public UnityEvent<LoveTriggerRequest> OnTriggerCompleted;
        public System.Action OnEncounterStateChanged;

        // ── Internal ──────────────────────────────
        private EncounterState _state = EncounterState.Idle;
        private LoveTriggerRequest _activeRequest;
        private Coroutine _encounterCoroutine;
        private ILoveTriggerNetworkService _network;

        // ── Properties ────────────────────────────
        public EncounterState State => _state;

        // ── Lifecycle ─────────────────────────────
        private void Start()
        {
            _network = FindObjectOfType<NetworkedLoveTriggerManager>();
            SetupUIElements();
        }

        // ── Public API ────────────────────────────

        /// <summary>
        /// Initiates a trigger request from a local player.
        /// Entry point for player-driven interactions.
        /// </summary>
        public void InitiateTrigger(LoveTriggerSO trigger, GameObject initiator, GameObject partner = null)
        {
            if (_state != EncounterState.Idle)
            {
                Debug.Log($"[InteractableObject] Cannot initiate — state is {_state}");
                return;
            }

            var request = new LoveTriggerRequest(trigger, initiator, partner);
            if (_network != null)
            {
                request.InitiatorPlayerRef = _network.LocalPlayerRef;
            }

            _encounterCoroutine = StartCoroutine(EncounterCoroutine(request));
        }

        /// <summary>Aborts the current encounter and returns to Idle.</summary>
        public void AbortEncounter()
        {
            if (_encounterCoroutine != null)
            {
                StopCoroutine(_encounterCoroutine);
                _encounterCoroutine = null;
            }
            TransitionTo(EncounterState.Idle);
            _animationController?.StopCurrent();
        }

        // ── Encounter state machine ────────────────
        private IEnumerator EncounterCoroutine(LoveTriggerRequest request)
        {
            _activeRequest = request;

            // DETECTION
            TransitionTo(EncounterState.Detection);
            yield return new WaitForSeconds(0.1f); // brief LOS / range check pause

            // PROMPTED
            TransitionTo(EncounterState.Prompted);
            ShowTriggerSelectionUI(_availableTriggers);
            // (UI calls back via SelectTrigger — we wait here)
            yield return new WaitUntil(() => _activeRequest.Trigger != null);

            // CONSENT_PENDING
            TransitionTo(EncounterState.ConsentPending);
            bool consentGranted = true;

            if (request.RequiresConsent && _network != null && request.HasPartner)
            {
                var consentTask = _network.RequestConsentAsync(
                    request.PartnerPlayerRef, request, _consentTimeoutSeconds);

                // Await async task from coroutine
                while (!consentTask.IsCompleted) yield return null;
                consentGranted = consentTask.Result;
            }

            if (!consentGranted)
            {
                Debug.Log($"[InteractableObject] Consent denied for {request.Trigger.triggerID}");
                OnTriggerDenied?.Invoke(request);
                TransitionTo(EncounterState.Idle);
                yield break;
            }

            // EXECUTING
            TransitionTo(EncounterState.Executing);
            OnTriggerExecuted?.Invoke(request);

            // Broadcast to all clients
            _network?.BroadcastTriggerExecution(new TriggerExecutionData
            {
                TriggerID          = request.Trigger.triggerID,
                InitiatorPlayerRef = request.InitiatorPlayerRef,
                PartnerPlayerRef   = request.PartnerPlayerRef,
                TimestampUtc       = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });

            // Drive animation or timeline
            if (request.Trigger is EnhancedLoveTriggerSO enhanced && enhanced.timelineAsset != null)
            {
                _timelineController?.PlayTimeline(enhanced, request.Initiator, request.Partner);
                yield return new WaitUntil(() => !_timelineController || !_timelineController.isActiveAndEnabled);
            }
            else if (_animationController != null)
            {
                bool done = false;
                _animationController.OnAnimationComplete = () => done = true;
                _animationController.PlayAnimation(request.Trigger.singleAnimation, request.Trigger);
                yield return new WaitUntil(() => done);
            }

            // RESTORING
            TransitionTo(EncounterState.Restoring);
            yield return new WaitForSeconds(request.Trigger.vrikBlendInDuration);

            // Complete
            _network?.BroadcastTriggerCompletion(new TriggerCompletionData
            {
                TriggerID        = request.Trigger.triggerID,
                CompletedNormally = true
            });

            OnTriggerCompleted?.Invoke(request);
            TransitionTo(EncounterState.Idle);
            _activeRequest     = null;
            _encounterCoroutine = null;
        }

        // ── State helpers ─────────────────────────
        private void TransitionTo(EncounterState newState)
        {
            Debug.Log($"[InteractableObject] {gameObject.name}: {_state} → {newState}");
            _state = newState;
            _activeRequest?.Let(r => r.State = newState);
            OnEncounterStateChanged?.Invoke();
        }

        // ── UI hooks (protected virtual — DEBT-D fix) ──
        /// <summary>
        /// Override in subclasses to inject custom UI setup.
        /// Protected virtual eliminates need for InteractableObjectFix.cs.
        /// </summary>
        protected virtual void SetupUIElements()
        {
            Debug.Log($"[InteractableObject] SetupUIElements on {gameObject.name}");
        }

        /// <summary>Show trigger selection UI. Override to implement your UI layer.</summary>
        protected virtual void ShowTriggerSelectionUI(List<LoveTriggerSO> triggers)
        {
            // TODO: Implement your UI here (world-space panel, VR pointer, etc.)
            Debug.Log($"[InteractableObject] ShowTriggerSelectionUI — {triggers.Count} triggers available.");
        }

        // ── Gizmos ────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }

    // ── Small extension for null-safe lambda calls ──
    internal static class Extensions
    {
        public static void Let<T>(this T obj, System.Action<T> action) where T : class
        {
            if (obj != null) action(obj);
        }
    }
}
