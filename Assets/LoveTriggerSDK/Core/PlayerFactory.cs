using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Instantiates and configures player GameObjects from a PlayerConfiguration.
    /// Called by PlayerSpawnManager on player join.
    /// </summary>
    public class PlayerFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Base player prefab. Must contain PlayerConfiguration component.")]
        [SerializeField] private GameObject _playerPrefab;

        [Header("Body Assembly")]
        [Tooltip("Root transform where body mesh is attached.")]
        [SerializeField] private Transform _bodyAttachPoint;

        // ── Public API ────────────────────────────

        /// <summary>
        /// Instantiates a player from config and returns the root GameObject.
        /// </summary>
        public GameObject CreatePlayer(PlayerConfiguration config, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (_playerPrefab == null)
            {
                Debug.LogError("[PlayerFactory] Player prefab is not assigned.");
                return null;
            }

            var playerGO = Instantiate(_playerPrefab, spawnPosition, spawnRotation);
            playerGO.name = $"Player_{config.displayName}_{config.networkPlayerRef}";

            // Copy configuration onto the spawned object's component
            var playerConfig = playerGO.GetComponent<PlayerConfiguration>();
            if (playerConfig == null)
                playerConfig = playerGO.AddComponent<PlayerConfiguration>();

            playerConfig.playerID        = config.playerID;
            playerConfig.displayName     = config.displayName;
            playerConfig.bodyConfig      = config.bodyConfig;
            playerConfig.networkPlayerRef = config.networkPlayerRef;
            playerConfig.defaultConsentType  = config.defaultConsentType;
            playerConfig.hapticDeviceID  = config.hapticDeviceID;
            playerConfig.globalHapticIntensity = config.globalHapticIntensity;

            // Attach body mesh
            AssembleBody(playerGO, config.bodyConfig);

            Debug.Log($"[PlayerFactory] Created player: {playerGO.name}");
            return playerGO;
        }

        // ── Internal ──────────────────────────────
        private void AssembleBody(GameObject playerGO, AvatarBodyConfig bodyConfig)
        {
            if (bodyConfig == null || bodyConfig.bodyPrefab == null) return;

            var attachPoint = _bodyAttachPoint != null
                ? _bodyAttachPoint
                : playerGO.transform;

            // Clear existing body children
            foreach (Transform child in attachPoint)
                Destroy(child.gameObject);

            Instantiate(bodyConfig.bodyPrefab, attachPoint);

            // Attach optional genital prefab
            if (bodyConfig.genitalPrefab != null)
                Instantiate(bodyConfig.genitalPrefab, attachPoint);

            Debug.Log($"[PlayerFactory] Body assembled: {bodyConfig.configName} ({bodyConfig.genderExpression})");
        }
    }
}
