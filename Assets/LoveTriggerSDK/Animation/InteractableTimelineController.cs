using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Cinemachine;
using RootMotion.FinalIK;

namespace LTSystem.Animation
{
    using LTSystem.Core;

    /// <summary>
    /// Drives Timeline-based encounter sequences for EnhancedLoveTriggerSO.
    /// Handles character track binding, Cinemachine VCam management,
    /// and VRIK restoration.
    ///
    /// Entry point for all Timeline encounters — never drive PlayableDirector
    /// directly from InteractableObject. Always go through this controller.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class InteractableTimelineController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────
        [Header("VCam Settings")]
        [Tooltip("Default gameplay VCam priority (must be lower than encounter priority).")]
        [SerializeField] private int _gameplayVCamPriority = 10;

        [Header("VRIK")]
        [SerializeField] private VRIK _initiatorVRIK;
        [SerializeField] private VRIK _partnerVRIK;

        // ── Internal ──────────────────────────────
        private PlayableDirector   _director;
        private CinemachineVirtualCamera _spawnedVCam;
        private Coroutine          _currentSequence;
        private bool               _isPlaying;

        // ── Events ────────────────────────────────
        public System.Action OnTimelineComplete;

        // ── Lifecycle ─────────────────────────────
        private void Awake() => _director = GetComponent<PlayableDirector>();

        private void OnDestroy() => StopCurrent();

        // ── Public API ────────────────────────────

        /// <summary>
        /// Plays the TimelineAsset from the given trigger with source/target bound.
        /// </summary>
        public void PlayTimeline(EnhancedLoveTriggerSO trigger, GameObject source, GameObject target)
        {
            if (_isPlaying) StopCurrent();
            _currentSequence = StartCoroutine(PlayTimelineCoroutine(trigger, source, target));
        }

        /// <summary>Stops playback and restores state immediately.</summary>
        public void StopCurrent()
        {
            if (_currentSequence != null)
            {
                StopCoroutine(_currentSequence);
                _currentSequence = null;
            }
            StartCoroutine(RestorePlayerState());
        }

        // ── Sequence coroutine ────────────────────
        private IEnumerator PlayTimelineCoroutine(
            EnhancedLoveTriggerSO trigger, GameObject source, GameObject target)
        {
            _isPlaying = true;

            try
            {
                // Phase 1: blend VRIK out on both participants
                yield return BlendVRIKOut(_initiatorVRIK, trigger.vrikBlendOutDuration);
                if (_partnerVRIK != null)
                    yield return BlendVRIKOut(_partnerVRIK, trigger.vrikBlendOutDuration);

                // Phase 2: spawn and activate encounter VCam
                if (trigger.vcamPrefab != null)
                    SpawnEncounterVCam(trigger.vcamPrefab, trigger.vcamPriority);

                // Phase 3: bind Timeline tracks dynamically
                if (trigger.timelineAsset != null)
                {
                    _director.playableAsset = trigger.timelineAsset;
                    BindCharacterTracks(_director, source, target);
                    _director.Play();

                    // Wait for timeline to complete
                    yield return new WaitUntil(() =>
                        _director.state == PlayState.Paused
                        || _director.time >= _director.duration - 0.05f);
                }
                else
                {
                    Debug.LogWarning($"[InteractableTimelineController] No TimelineAsset on trigger {trigger.triggerID}");
                    yield return null;
                }
            }
            finally
            {
                // Always runs — even on interruption
                yield return RestorePlayerState();
                Debug.Log($"[InteractableTimelineController] Timeline complete: {trigger.triggerID}");
                OnTimelineComplete?.Invoke();
            }
        }

        // ── Timeline track binding ─────────────────
        /// <summary>
        /// Binds Source/Target tracks dynamically so Timeline assets never
        /// need hardcoded scene references.
        /// </summary>
        private void BindCharacterTracks(PlayableDirector director, GameObject source, GameObject target)
        {
            foreach (var binding in director.playableAsset.outputs)
            {
                if (binding.streamName.Contains("Source"))
                    director.SetGenericBinding(binding.sourceObject, source);
                else if (binding.streamName.Contains("Target"))
                    director.SetGenericBinding(binding.sourceObject, target);
            }
            Debug.Log("[InteractableTimelineController] Tracks bound: Source=" +
                      source.name + " Target=" + target.name);
        }

        // ── VCam management ───────────────────────
        private void SpawnEncounterVCam(GameObject vcamPrefab, int priority)
        {
            _spawnedVCam = Instantiate(vcamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
            if (_spawnedVCam != null)
            {
                _spawnedVCam.Priority = priority;
                Debug.Log($"[InteractableTimelineController] Encounter VCam activated, priority={priority}");
            }
        }

        private void DespawnEncounterVCam()
        {
            if (_spawnedVCam != null)
            {
                Destroy(_spawnedVCam.gameObject);
                _spawnedVCam = null;
                Debug.Log("[InteractableTimelineController] Encounter VCam destroyed, gameplay priority restored.");
            }
        }

        // ── State restoration ─────────────────────
        private IEnumerator RestorePlayerState()
        {
            _director.Stop();
            DespawnEncounterVCam();

            yield return BlendVRIKIn(_initiatorVRIK, 0.4f);
            if (_partnerVRIK != null)
                yield return BlendVRIKIn(_partnerVRIK, 0.4f);

            _isPlaying = false;
            _currentSequence = null;
        }

        // ── VRIK helpers (mirrored from UniversalAnimationController) ──
        private IEnumerator BlendVRIKOut(VRIK vrik, float duration)
        {
            if (vrik == null) yield break;
            float elapsed = 0f, start = vrik.solver.IKPositionWeight;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                vrik.solver.IKPositionWeight = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            vrik.solver.IKPositionWeight = 0f;
        }

        private IEnumerator BlendVRIKIn(VRIK vrik, float duration, float target = 1f)
        {
            if (vrik == null) yield break;
            float elapsed = 0f, start = vrik.solver.IKPositionWeight;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                vrik.solver.IKPositionWeight = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            vrik.solver.IKPositionWeight = target;
        }
    }
}
