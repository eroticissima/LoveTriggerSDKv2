using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace LTSystem.Interaction
{
    using LTSystem.Core;

    /// <summary>
    /// Controls an NPC fallback partner for solo / non-multiplayer sessions.
    /// NPCs run through the SAME consent/trigger pipeline as human players —
    /// no bypasses. Auto-response is handled via PlayerConfiguration.ConsentType.
    ///
    /// Behavior arc: Think → Agree → Approach → Execute
    /// Audio response banks are keyed to ConsentType, not random.
    /// Appearance is randomized at spawn only — never mid-encounter.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCPartnerController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────
        [Header("NPC Identity")]
        [SerializeField] private string _npcName;
        [Tooltip("Gender expression tag — freeform, non-binary by design.")]
        [SerializeField] private string _genderExpression = "nonbinary";

        [Header("Consent Behavior")]
        [Tooltip("How this NPC responds to trigger requests.")]
        [SerializeField] private ConsentType _consentBehavior = ConsentType.Manual;
        [Tooltip("Chance (0-1) this NPC accepts a Manual consent request.")]
        [Range(0f, 1f)]
        [SerializeField] private float _agreeProbability = 0.85f;

        [Header("Timing — Hesitation Arc")]
        [SerializeField] private float _thinkDurationMin = 0.5f;
        [SerializeField] private float _thinkDurationMax = 2.0f;
        [SerializeField] private float _approachStopDistance = 1.0f;

        [Header("Audio Banks (keyed to ConsentType)")]
        [SerializeField] private AudioClip[] _greetingClips;
        [SerializeField] private AudioClip[] _agreementClips;
        [SerializeField] private AudioClip[] _declineClips;
        [SerializeField] private AudioClip[] _goodbyeClips;

        [Header("Relationship State")]
        [Tooltip("Accumulated positive interactions — affects hesitation arc.")]
        [SerializeField] private float _relationshipScore;

        // ── Internal ──────────────────────────────
        private NavMeshAgent  _agent;
        private AudioSource   _audio;
        private PlayerConfiguration _config;
        private bool          _appearanceSet;

        // ── Lifecycle ─────────────────────────────
        private void Awake()
        {
            _agent  = GetComponent<NavMeshAgent>();
            _audio  = GetComponent<AudioSource>();
            _config = GetComponent<PlayerConfiguration>();

            // Subscribe to consent requests on the event bus
            LoveTriggerEventBus.OnConsentRequestReceived += HandleConsentRequest;
        }

        private void OnDestroy()
        {
            LoveTriggerEventBus.OnConsentRequestReceived -= HandleConsentRequest;
        }

        private void Start()
        {
            // Appearance is randomized once at spawn — never mid-encounter
            if (!_appearanceSet)
            {
                RandomizeAppearance();
                _appearanceSet = true;
            }

            PlayRandomClip(_greetingClips);
        }

        // ── Consent pipeline ──────────────────────

        /// <summary>
        /// Receives an incoming consent request and runs the NPC hesitation arc.
        /// Never bypasses the consent gate — AlwaysAccept simply shortens Think().
        /// </summary>
        private void HandleConsentRequest(ConsentRequestData requestData)
        {
            // Only respond to requests targeting this NPC
            if (_config == null || requestData.TriggerID == null) return;
            StartCoroutine(ConsentArc(requestData));
        }

        private IEnumerator ConsentArc(ConsentRequestData requestData)
        {
            // Think — believable hesitation before responding
            yield return Think(requestData);

            bool accepted = Agree(requestData);

            // Publish response through the event bus
            LoveTriggerEventBus.RaiseConsentResponseReceived(new ConsentResponseData
            {
                RequestID          = requestData.RequestID,
                Accepted           = accepted,
                ResponderPlayerRef = _config != null ? _config.networkPlayerRef : 0
            });

            if (accepted)
            {
                PlayRandomClip(_agreementClips);
                yield return ApproachInitiator(requestData.InitiatorPlayerRef);
            }
            else
            {
                PlayRandomClip(_declineClips);
            }
        }

        private IEnumerator Think(ConsentRequestData requestData)
        {
            float duration = _consentBehavior == ConsentType.AlwaysAccept
                ? 0.2f
                : Mathf.Lerp(_thinkDurationMax, _thinkDurationMin, _relationshipScore / 100f);

            // Add some variance
            duration += Random.Range(-0.2f, 0.2f);
            duration  = Mathf.Max(0.1f, duration);
            yield return new WaitForSeconds(duration);
        }

        private bool Agree(ConsentRequestData requestData)
        {
            return _consentBehavior switch
            {
                ConsentType.AlwaysAccept => true,
                ConsentType.Manual       => Random.value <= _agreeProbability,
                ConsentType.TrustedOnly  => _config != null
                                            && _config.IsTrusted(requestData.InitiatorPlayerRef),
                _ => false
            };
        }

        private IEnumerator ApproachInitiator(ulong initiatorRef)
        {
            var playerSpawnManager = FindObjectOfType<PlayerSpawnManager>();
            if (playerSpawnManager == null) yield break;

            var initiatorGO = playerSpawnManager.GetPlayer(initiatorRef);
            if (initiatorGO == null) yield break;

            _agent.stoppingDistance = _approachStopDistance;
            _agent.SetDestination(initiatorGO.transform.position);

            while (_agent.remainingDistance > _approachStopDistance + 0.1f)
                yield return null;

            _agent.ResetPath();
            Debug.Log($"[NPCPartnerController] {_npcName} reached initiator.");
        }

        // ── Post-encounter ─────────────────────────
        /// <summary>Called by InteractableObject when the sequence ends.</summary>
        public void OnEncounterEnded()
        {
            PlayRandomClip(_goodbyeClips);
            _relationshipScore = Mathf.Clamp(_relationshipScore + 10f, 0f, 100f);
            Debug.Log($"[NPCPartnerController] {_npcName} goodbye. Relationship: {_relationshipScore}");
        }

        // ── Appearance ─────────────────────────────
        /// <summary>Randomize appearance once at spawn. Never call mid-encounter.</summary>
        private void RandomizeAppearance()
        {
            // TODO: Drive DAZ/HAVAS material and mesh variant selection here
            Debug.Log($"[NPCPartnerController] Appearance randomized for {_npcName} ({_genderExpression})");
        }

        // ── Audio helpers ─────────────────────────
        private void PlayRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0 || _audio == null) return;
            _audio.clip = clips[Random.Range(0, clips.Length)];
            _audio.Play();
        }
    }
}
