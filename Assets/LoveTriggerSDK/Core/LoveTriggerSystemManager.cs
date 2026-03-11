using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LTSystem.Core
{
    /// <summary>
    /// Bootstrap manager. Place one instance in your persistent scene.
    /// Initializes all SDK subsystems in order and sets IsInitialized.
    ///
    /// DEBT-A FIXED: Removed premature yield break that previously made
    /// IsInitialized = true unreachable.
    /// </summary>
    public class LoveTriggerSystemManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────
        public static LoveTriggerSystemManager Instance { get; private set; }

        // ── State ──────────────────────────────────
        /// <summary>True once all subsystems have initialized successfully.</summary>
        public bool IsInitialized { get; private set; }

        // ── Inspector ─────────────────────────────
        [Header("Subsystem References")]
        [SerializeField] private LoveTriggerDatabase _database;
        [SerializeField] private PlayerSpawnManager  _spawnManager;

        [Header("Settings")]
        [Tooltip("Seconds to wait between each subsystem init step.")]
        [SerializeField] private float _initStepDelay = 0.05f;

        [Header("Events")]
        public UnityEvent OnSystemInitialized;
        public UnityEvent<string> OnSystemInitFailed;

        // ── Lifecycle ─────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => StartCoroutine(InitializeSystemCoroutine());

        // ── Initialization coroutine ──────────────
        private IEnumerator InitializeSystemCoroutine()
        {
            Debug.Log("[LoveTriggerSystemManager] Starting SDK initialization...");

            // Step 1 — Validate required references
            if (_database == null)
            {
                string err = "LoveTriggerDatabase reference is missing.";
                Debug.LogError($"[LoveTriggerSystemManager] INIT FAILED: {err}");
                OnSystemInitFailed?.Invoke(err);
                yield break;            // ← Only yield break on genuine failure
            }
            yield return new WaitForSeconds(_initStepDelay);

            // Step 2 — Database is already self-initializing via its own Awake.
            // Additional validation can go here.
            Debug.Log("[LoveTriggerSystemManager] Database ready.");
            yield return new WaitForSeconds(_initStepDelay);

            // Step 3 — Platform detection
            var platform = PlatformManager.Current;
            Debug.Log($"[LoveTriggerSystemManager] Platform detected: {platform}");
            yield return new WaitForSeconds(_initStepDelay);

            // Step 4 — Spawn manager
            if (_spawnManager != null)
                _spawnManager.Initialize();
            yield return new WaitForSeconds(_initStepDelay);

            // ── DEBT-A FIX: IsInitialized = true is now reachable ──
            IsInitialized = true;
            Debug.Log("[LoveTriggerSystemManager] SDK initialized successfully.");
            OnSystemInitialized?.Invoke();
        }
    }
}
