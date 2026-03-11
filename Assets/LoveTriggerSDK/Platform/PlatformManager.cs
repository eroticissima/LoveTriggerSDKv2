using UnityEngine;
using UnityEngine.XR.Management;

namespace LTSystem.Core
{
    /// <summary>
    /// Authoritative single-source platform detection.
    ///
    /// DEBT-B RESOLVED: Replaces the split between PlatformDetectionSystem
    /// and EnhancedPlatformDetectionSystem. This class is the only platform
    /// authority. Delete both legacy files once migrated.
    ///
    /// Access via PlatformManager.Current anywhere in the SDK.
    /// </summary>
    public static class PlatformManager
    {
        private static Platform? _cached;

        /// <summary>Detected platform. Evaluated once, then cached.</summary>
        public static Platform Current
        {
            get
            {
                if (_cached.HasValue) return _cached.Value;
                _cached = Detect();
                Debug.Log($"[PlatformManager] Detected platform: {_cached.Value}");
                return _cached.Value;
            }
        }

        /// <summary>Force re-detection (e.g. after XR mode toggle at runtime).</summary>
        public static void Invalidate() => _cached = null;

        // ── Detection logic ───────────────────────
        private static Platform Detect()
        {
#if UNITY_ANDROID || UNITY_IOS
            // Check XR first — Android/iOS can be a standalone headset (Quest, Pico)
            if (IsXRActive()) return Platform.XR;
            return Platform.Mobile;
#elif UNITY_STANDALONE || UNITY_EDITOR
            if (IsXRActive()) return Platform.XR;
            return Platform.Desktop;
#elif UNITY_GAMECORE || UNITY_PS5 || UNITY_PS4 || UNITY_SWITCH
            return Platform.Console;
#else
            return Platform.Desktop;
#endif
        }

        private static bool IsXRActive()
        {
            var xrSettings = XRGeneralSettings.Instance;
            return xrSettings != null
                && xrSettings.Manager != null
                && xrSettings.Manager.isInitializationComplete;
        }
    }
}
