using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Per-player configuration data: avatar, consent preferences,
    /// haptic device bindings, and relationship state.
    /// Created by PlayerFactory and stored on the player GameObject.
    /// </summary>
    public class PlayerConfiguration : MonoBehaviour
    {
        [Header("Identity")]
        public string playerID;
        public string displayName;

        [Header("Avatar")]
        [Tooltip("Body configuration — gender-neutral, fully customizable.")]
        public AvatarBodyConfig bodyConfig;

        [Header("Consent Settings")]
        [Tooltip("How this player handles incoming trigger requests.")]
        public ConsentType defaultConsentType = ConsentType.Manual;
        [Tooltip("Player refs that are auto-trusted for TrustedOnly triggers.")]
        public ulong[] trustedPlayerRefs;

        [Header("Haptics")]
        [Tooltip("Connected haptic device ID. Empty if none.")]
        public string hapticDeviceID;
        [Range(0f, 1f)] public float globalHapticIntensity = 0.75f;

        [Header("Network")]
        /// <summary>Photon Fusion player ref (ulong). Set by PlayerFactory.</summary>
        public ulong networkPlayerRef;

        // ── Helpers ───────────────────────────────

        /// <summary>Returns true if the given player ref is in the trusted list.</summary>
        public bool IsTrusted(ulong playerRef)
        {
            if (trustedPlayerRefs == null) return false;
            foreach (var r in trustedPlayerRefs)
                if (r == playerRef) return true;
            return false;
        }

        /// <summary>Evaluates whether an incoming request should auto-accept.</summary>
        public bool ShouldAutoAccept(LoveTriggerRequest request)
        {
            return defaultConsentType == ConsentType.AlwaysAccept
                || (defaultConsentType == ConsentType.TrustedOnly
                    && IsTrusted(request.InitiatorPlayerRef));
        }
    }
}
