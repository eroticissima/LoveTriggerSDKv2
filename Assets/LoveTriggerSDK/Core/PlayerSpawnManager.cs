using System.Collections.Generic;
using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Manages spawn points and tracks all active player instances.
    /// Works alongside Photon Fusion's spawning — call SpawnPlayer()
    /// from your NetworkedLoveTriggerManager after Fusion instantiation.
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private Transform[] _spawnPoints;

        [Header("References")]
        [SerializeField] private PlayerFactory _factory;

        // Track active players by network ref
        private readonly Dictionary<ulong, GameObject> _activePlayers
            = new Dictionary<ulong, GameObject>();

        private int _nextSpawnIndex;

        // ── Lifecycle ─────────────────────────────
        /// <summary>Called by LoveTriggerSystemManager during boot.</summary>
        public void Initialize()
        {
            _activePlayers.Clear();
            _nextSpawnIndex = 0;
            Debug.Log($"[PlayerSpawnManager] Initialized. {_spawnPoints?.Length ?? 0} spawn points available.");
        }

        // ── Public API ────────────────────────────

        /// <summary>Spawns a player at the next available spawn point.</summary>
        public GameObject SpawnPlayer(PlayerConfiguration config)
        {
            if (_factory == null)
            {
                Debug.LogError("[PlayerSpawnManager] PlayerFactory reference missing.");
                return null;
            }

            var (pos, rot) = GetNextSpawnTransform();
            var playerGO   = _factory.CreatePlayer(config, pos, rot);

            if (playerGO != null)
                _activePlayers[config.networkPlayerRef] = playerGO;

            return playerGO;
        }

        /// <summary>Removes and destroys a player by network ref.</summary>
        public void DespawnPlayer(ulong networkPlayerRef)
        {
            if (_activePlayers.TryGetValue(networkPlayerRef, out var go))
            {
                Destroy(go);
                _activePlayers.Remove(networkPlayerRef);
                Debug.Log($"[PlayerSpawnManager] Despawned player ref: {networkPlayerRef}");
            }
        }

        /// <summary>Returns the player GameObject for a network ref, or null.</summary>
        public GameObject GetPlayer(ulong networkPlayerRef)
        {
            _activePlayers.TryGetValue(networkPlayerRef, out var go);
            return go;
        }

        // ── Internal ──────────────────────────────
        private (Vector3 pos, Quaternion rot) GetNextSpawnTransform()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                return (Vector3.zero, Quaternion.identity);

            var sp = _spawnPoints[_nextSpawnIndex % _spawnPoints.Length];
            _nextSpawnIndex++;
            return (sp.position, sp.rotation);
        }
    }
}
