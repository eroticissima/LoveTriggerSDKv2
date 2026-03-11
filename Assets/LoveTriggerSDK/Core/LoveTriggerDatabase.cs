using System.Collections.Generic;
using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Runtime registry of all LoveTriggerSO assets.
    /// Populated on boot by LoveTriggerSystemManager.
    /// Use LoveTriggerDatabase.Instance.Get(id) for lookups.
    /// </summary>
    public class LoveTriggerDatabase : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────
        public static LoveTriggerDatabase Instance { get; private set; }

        [Header("Registered Triggers")]
        [Tooltip("Drag all LoveTriggerSO assets here, or let SystemManager auto-populate via Resources.")]
        [SerializeField] private LoveTriggerSO[] _triggers;

        private readonly Dictionary<string, LoveTriggerSO> _lookup
            = new Dictionary<string, LoveTriggerSO>();

        // ── Lifecycle ─────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[LoveTriggerDatabase] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookup();
        }

        // ── Public API ────────────────────────────

        /// <summary>Returns the trigger with the given ID, or null if not found.</summary>
        public LoveTriggerSO Get(string triggerID)
        {
            if (_lookup.TryGetValue(triggerID, out var trigger)) return trigger;
            Debug.LogWarning($"[LoveTriggerDatabase] Trigger not found: {triggerID}");
            return null;
        }

        /// <summary>Returns all registered triggers.</summary>
        public IEnumerable<LoveTriggerSO> GetAll() => _lookup.Values;

        /// <summary>Returns all triggers matching a tag.</summary>
        public List<LoveTriggerSO> GetByTag(string tag)
        {
            var results = new List<LoveTriggerSO>();
            foreach (var t in _lookup.Values)
            {
                if (t.tags == null) continue;
                foreach (var tg in t.tags)
                    if (tg == tag) { results.Add(t); break; }
            }
            return results;
        }

        /// <summary>
        /// Registers a trigger at runtime (e.g. downloaded content).
        /// Overwrites existing entry if ID matches.
        /// </summary>
        public void Register(LoveTriggerSO trigger)
        {
            if (trigger == null || string.IsNullOrEmpty(trigger.triggerID)) return;
            _lookup[trigger.triggerID] = trigger;
            Debug.Log($"[LoveTriggerDatabase] Registered trigger: {trigger.triggerID}");
        }

        // ── Internal ──────────────────────────────
        private void BuildLookup()
        {
            _lookup.Clear();

            // Inspector-assigned
            if (_triggers != null)
                foreach (var t in _triggers)
                    if (t != null && !string.IsNullOrEmpty(t.triggerID))
                        _lookup[t.triggerID] = t;

            // Auto-load from Resources/LoveTriggers/
            var fromResources = Resources.LoadAll<LoveTriggerSO>("LoveTriggers");
            foreach (var t in fromResources)
                if (!string.IsNullOrEmpty(t.triggerID))
                    _lookup[t.triggerID] = t;

            Debug.Log($"[LoveTriggerDatabase] Built lookup — {_lookup.Count} triggers registered.");
        }
    }
}
