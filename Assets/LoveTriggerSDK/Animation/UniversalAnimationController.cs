using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using RootMotion.FinalIK;

namespace LTSystem.Animation
{
    using LTSystem.Core;

    /// <summary>
    /// Drives animation playback for a single avatar during a LoveTrigger sequence.
    /// Handles VRIK blending, Playables graph construction, and state restoration.
    ///
    /// Attach one instance per avatar root GameObject.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class UniversalAnimationController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────
        [Header("IK")]
        [Tooltip("VRIK component on this avatar. Auto-found if null.")]
        [SerializeField] private VRIK _vrik;

        [Header("Timing")]
        [Tooltip("Default blend duration if not specified in AnimationData.")]
        [SerializeField] private float _defaultBlendDuration = 0.3f;

        // ── Internal ──────────────────────────────
        private Animator        _animator;
        private PlayableGraph   _graph;
        private Coroutine       _currentSequence;
        private Coroutine       _vrikBlendCoroutine;
        private bool            _isPlaying;
        private float           _vrikWeightAtSequenceStart;

        // ── Events ────────────────────────────────
        public System.Action OnAnimationComplete;
        public System.Action OnAnimationInterrupted;

        // ── Lifecycle ─────────────────────────────
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_vrik == null) _vrik = GetComponentInChildren<VRIK>();
        }

        private void OnDestroy()
        {
            if (_graph.IsValid()) _graph.Destroy();
        }

        // ── Public API ────────────────────────────

        /// <summary>
        /// Plays an animation from AnimationData on this avatar.
        /// VRIK is blended out for the duration, then restored.
        /// </summary>
        public void PlayAnimation(AnimationData data, LoveTriggerSO trigger)
        {
            if (_isPlaying) StopCurrent();

            _currentSequence = StartCoroutine(
                PlaySequenceCoroutine(data, trigger.vrikBlendOutDuration, trigger.vrikBlendInDuration));
        }

        /// <summary>Stops the current animation and restores state immediately.</summary>
        public void StopCurrent()
        {
            if (_currentSequence != null)
            {
                StopCoroutine(_currentSequence);
                _currentSequence = null;
            }
            RestoreState();
        }

        // ── Sequence coroutine ────────────────────
        private IEnumerator PlaySequenceCoroutine(AnimationData data, float blendOut, float blendIn)
        {
            _isPlaying = true;
            _vrikWeightAtSequenceStart = _vrik != null ? _vrik.solver.IKPositionWeight : 1f;

            try
            {
                // Phase 1: blend VRIK out
                yield return BlendVRIKOut(_vrik, blendOut > 0 ? blendOut : _defaultBlendDuration);

                // Phase 2: play animation
                switch (data.method)
                {
                    case AnimationMethod.PlayableGraph:
                        yield return PlayViaPlayables(data);
                        break;
                    case AnimationMethod.AnimatorOverride:
                        yield return PlayViaOverride(data);
                        break;
                    case AnimationMethod.SeparateAnimator:
                        yield return PlayViaSeparateAnimator(data);
                        break;
                }
            }
            finally
            {
                // Phase 3: always restore, even on interruption
                yield return BlendVRIKIn(_vrik, blendIn > 0 ? blendIn : _defaultBlendDuration,
                                          _vrikWeightAtSequenceStart);
                _isPlaying = false;
                _currentSequence = null;
                OnAnimationComplete?.Invoke();
                Debug.Log($"[UniversalAnimationController] Sequence complete on {gameObject.name}");
            }
        }

        // ── Animation method implementations ──────

        private IEnumerator PlayViaPlayables(AnimationData data)
        {
            if (data.clip == null) yield break;

            if (_graph.IsValid()) _graph.Destroy();
            _graph = PlayableGraph.Create($"LT_{data.clip.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var clipPlayable = AnimationClipPlayable.Create(_graph, data.clip);
            var output       = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
            output.SetSourcePlayable(clipPlayable);

            _graph.Play();

            // Wait for clip to finish
            float elapsed = 0f;
            while (elapsed < data.clip.length - data.blendOut)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _graph.Destroy();
        }

        private IEnumerator PlayViaOverride(AnimationData data)
        {
            if (data.clip == null || string.IsNullOrEmpty(data.stateName)) yield break;

            var overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            // TODO: Find the correct clip slot by stateName and replace
            _animator.runtimeAnimatorController = overrideController;
            _animator.Play(data.stateName);

            yield return new WaitForSeconds(data.clip.length);
        }

        private IEnumerator PlayViaSeparateAnimator(AnimationData data)
        {
            // TODO: Locate or spawn the dedicated Animator for this body segment
            // and drive it directly.
            Debug.LogWarning("[UniversalAnimationController] SeparateAnimator path not yet implemented.");
            yield return new WaitForSeconds(data.clip != null ? data.clip.length : 1f);
        }

        // ── VRIK blend helpers ────────────────────

        /// <summary>Lerps VRIK IKPositionWeight to 0 over duration.</summary>
        private IEnumerator BlendVRIKOut(VRIK vrik, float duration)
        {
            if (vrik == null) yield break;
            float elapsed = 0f;
            float start   = vrik.solver.IKPositionWeight;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                vrik.solver.IKPositionWeight = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            vrik.solver.IKPositionWeight = 0f;
            Debug.Log($"[UniversalAnimationController] VRIK blended out on {gameObject.name}");
        }

        /// <summary>Lerps VRIK IKPositionWeight back to targetWeight over duration.</summary>
        private IEnumerator BlendVRIKIn(VRIK vrik, float duration, float targetWeight = 1f)
        {
            if (vrik == null) yield break;
            float elapsed = 0f;
            float start   = vrik.solver.IKPositionWeight;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                vrik.solver.IKPositionWeight = Mathf.Lerp(start, targetWeight, elapsed / duration);
                yield return null;
            }
            vrik.solver.IKPositionWeight = targetWeight;
            Debug.Log($"[UniversalAnimationController] VRIK restored on {gameObject.name}");
        }

        private void RestoreState()
        {
            if (_vrik != null) _vrik.solver.IKPositionWeight = _vrikWeightAtSequenceStart;
            if (_graph.IsValid()) _graph.Destroy();
            _isPlaying = false;
        }
    }
}
