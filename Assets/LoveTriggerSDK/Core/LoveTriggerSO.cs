using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Authoring ScriptableObject for a single LoveTrigger.
    /// Create via Assets > LoveTriggerSDK > Create LoveTrigger.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLoveTrigger", menuName = "LoveTriggerSDK/LoveTrigger")]
    public class LoveTriggerSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique string ID used for database lookup.")]
        public string triggerID;
        [Tooltip("Display name shown in the trigger selection UI.")]
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Animation (Current)")]
        [Tooltip("Use this. animatorClip below is legacy-compat only.")]
        public AnimationData singleAnimation;
        public AnimationType animationType = AnimationType.SingleCharacter;
        public AnimationMethod animationMethod = AnimationMethod.PlayableGraph;

        [Header("Consent & Priority")]
        public ConsentType consentType = ConsentType.Manual;
        public TriggerPriority priority   = TriggerPriority.Normal;
        [Tooltip("If true, partner must explicitly accept before execution.")]
        public bool requiresConsent = true;

        [Header("Timing")]
        [Tooltip("Seconds to blend VRIK out at sequence start.")]
        public float vrikBlendOutDuration = 0.4f;
        [Tooltip("Seconds to blend VRIK in at sequence end.")]
        public float vrikBlendInDuration  = 0.4f;

        [Header("Tags")]
        public string[] tags;

        // ── Legacy compatibility ───────────────────
        // DO NOT use in new code. Exists only to avoid
        // breaking assets authored before AnimationData.
        [HideInInspector]
        [System.Obsolete("Use singleAnimation instead.")]
        public AnimationClip animatorClip;
    }
}
