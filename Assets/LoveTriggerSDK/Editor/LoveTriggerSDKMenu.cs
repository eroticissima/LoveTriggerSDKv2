#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace LTSystem.Editor
{
    /// <summary>
    /// Unity Editor menu items for LoveTriggerSDK.
    /// Accessible via the top menu: Eroticissima / LoveTriggerSDK
    /// </summary>
    public static class LoveTriggerSDKMenu
    {
        private const string MENU_ROOT   = "Eroticissima/LoveTriggerSDK/";
        private const string RESOURCES_PATH = "Assets/Resources/LoveTriggers";

        [MenuItem(MENU_ROOT + "SDK Dashboard (Web)")]
        public static void OpenDashboard()
        {
            Application.OpenURL("https://www.eroticissima.wtf/sdk.html");
        }

        [MenuItem(MENU_ROOT + "Discord (SDK Access)")]
        public static void OpenDiscord()
        {
            Application.OpenURL("https://discord.gg/KTCdKNnvGb");
        }

        [MenuItem(MENU_ROOT + "─────────────────────", false, 50)]
        public static void Separator1() { }

        [MenuItem(MENU_ROOT + "Create New LoveTrigger")]
        public static void CreateLoveTrigger()
        {
            EnsureResourcesFolder();
            var asset = ScriptableObject.CreateInstance<LTSystem.Core.LoveTriggerSO>();
            var path  = AssetDatabase.GenerateUniqueAssetPath(RESOURCES_PATH + "/NewLoveTrigger.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log($"[LoveTriggerSDK] Created LoveTriggerSO at {path}");
        }

        [MenuItem(MENU_ROOT + "Create New Enhanced LoveTrigger")]
        public static void CreateEnhancedLoveTrigger()
        {
            EnsureResourcesFolder();
            var asset = ScriptableObject.CreateInstance<LTSystem.Core.EnhancedLoveTriggerSO>();
            var path  = AssetDatabase.GenerateUniqueAssetPath(RESOURCES_PATH + "/NewEnhancedLoveTrigger.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem(MENU_ROOT + "─────────────────────", false, 100)]
        public static void Separator2() { }

        [MenuItem(MENU_ROOT + "Validate SDK Setup")]
        public static void ValidateSetup()
        {
            var issues = 0;

            // Check for SystemManager in scene
            var manager = Object.FindObjectOfType<LTSystem.Core.LoveTriggerSystemManager>();
            if (manager == null)
            {
                Debug.LogWarning("[LoveTriggerSDK] ⚠ LoveTriggerSystemManager not found in scene.");
                issues++;
            }
            else Debug.Log("[LoveTriggerSDK] ✓ LoveTriggerSystemManager present.");

            // Check for Database
            var db = Object.FindObjectOfType<LTSystem.Core.LoveTriggerDatabase>();
            if (db == null)
            {
                Debug.LogWarning("[LoveTriggerSDK] ⚠ LoveTriggerDatabase not found in scene.");
                issues++;
            }
            else Debug.Log("[LoveTriggerSDK] ✓ LoveTriggerDatabase present.");

            // Check Resources folder
            if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
            {
                Debug.LogWarning("[LoveTriggerSDK] ⚠ Resources/LoveTriggers folder missing. Create it and add your LoveTriggerSO assets.");
                issues++;
            }
            else
            {
                var guids = AssetDatabase.FindAssets("t:LoveTriggerSO", new[] { RESOURCES_PATH });
                Debug.Log($"[LoveTriggerSDK] ✓ {guids.Length} LoveTriggerSO assets found in Resources.");
            }

            if (issues == 0)
                EditorUtility.DisplayDialog("LoveTriggerSDK", "✓ SDK setup looks good!", "OK");
            else
                EditorUtility.DisplayDialog("LoveTriggerSDK",
                    $"Found {issues} issue(s). Check the Console for details.", "OK");
        }

        [MenuItem(MENU_ROOT + "Documentation")]
        public static void OpenDocs()
        {
            Application.OpenURL("https://github.com/eroticissima/lovetriggersdk");
        }

        // ── Helpers ───────────────────────────────
        private static void EnsureResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
                AssetDatabase.CreateFolder("Assets/Resources", "LoveTriggers");
        }
    }
}
#endif
