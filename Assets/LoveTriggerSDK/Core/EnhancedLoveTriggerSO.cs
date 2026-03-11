using UnityEngine;
using UnityEngine.Events;

namespace LTSystem.Core
{
    /// <summary>
    /// Extended LoveTrigger with multi-animation support, haptics,
    /// audio cues, and Timeline integration. Inherits base authoring.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnhancedLoveTrigger",
                     menuName  = "LoveTriggerSDK/Enhanced LoveTrigger")]
    public class EnhancedLoveTriggerSO : LoveTriggerSO
    {
        [Header("Multi-Animation (Partner / Synchronized)")]
        [Tooltip("Animation played on the initiating body.")]
        public AnimationData initiatorAnimation;
        [Tooltip("Animation played on the partner body.")]
        public AnimationData partnerAnimation;
        [Tooltip("Optional additional animations for 3+ body scenes.")]
        public AnimationData[] extraAnimations;

        [Header("Haptics")]
        [Tooltip("Haptic pattern identifier string for connected toys.")]
        public string hapticPatternID;
        [Range(0f, 1f)] public float hapticIntensity = 0.5f;

        [Header("Audio")]
        [Tooltip("Ambient audio clip looped during the sequence.")]
        public AudioClip ambientClip;
        [Tooltip("One-shot audio played on trigger start.")]
        public AudioClip startStingClip;

        [Header("Timeline")]
        [Tooltip("If set, InteractableTimelineController will drive this asset.")]
        public UnityEngine.Timeline.TimelineAsset timelineAsset;

        [Header("Cinematic Camera")]
        [Tooltip("Cinemachine Virtual Camera prefab spawned during execution.")]
        public GameObject vcamPrefab;
        [Tooltip("Priority assigned to encounter VCam (default gameplay = 10).")]
        public int vcamPriority = 15;

        [Header("Unlock Conditions")]
        [Tooltip("Tags that must be present on interacting avatars.")]
        public string[] requiredAvatarTags;
        [Tooltip("Optional toy asset ID required to unlock this trigger.")]
        public string requiredToyID;
    }
}
