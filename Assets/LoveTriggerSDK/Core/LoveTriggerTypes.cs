using System;
using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>Defines how an animation is structured relative to participating bodies.</summary>
    public enum AnimationType
    {
        SingleCharacter,
        Partner,
        Synchronized
    }

    /// <summary>Defines how consent is handled for a given LoveTrigger.</summary>
    public enum ConsentType
    {
        Manual,
        TrustedOnly,
        AlwaysAccept
    }

    /// <summary>Scheduling priority for trigger requests.</summary>
    public enum TriggerPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>Which Unity animation system drives playback.</summary>
    public enum AnimationMethod
    {
        AnimatorOverride,
        PlayableGraph,
        SeparateAnimator
    }

    /// <summary>Runtime state of an encounter sequence.</summary>
    public enum EncounterState
    {
        Idle,
        Detection,
        Prompted,
        ConsentPending,
        Executing,
        Restoring
    }

    /// <summary>
    /// Authoritative platform enum. Single source of truth — replaces legacy
    /// PlatformDetectionSystem + EnhancedPlatformDetectionSystem split.
    /// </summary>
    public enum Platform
    {
        Desktop,
        XR,
        Console,
        Mobile
    }

    /// <summary>Describes a single animation clip with its method and timing metadata.</summary>
    [Serializable]
    public class AnimationData
    {
        public AnimationClip clip;
        public AnimationMethod method = AnimationMethod.PlayableGraph;
        [Tooltip("Blend-in duration in seconds.")] public float blendIn  = 0.25f;
        [Tooltip("Blend-out duration in seconds.")] public float blendOut = 0.25f;
        public string stateName;
    }

    /// <summary>
    /// Gender-neutral, fully customizable avatar body configuration.
    /// All character systems must support this without assumptions.
    /// </summary>
    [Serializable]
    public class AvatarBodyConfig
    {
        [Tooltip("Display name for this body configuration.")]
        public string configName = "Default";
        [Tooltip("Gender expression tag — freeform, non-binary by design.")]
        public string genderExpression = "nonbinary";
        public GameObject bodyPrefab;
        [Tooltip("Optional genital attachment prefab.")]
        public GameObject genitalPrefab;
        public string hapticDeviceId;
    }
}
