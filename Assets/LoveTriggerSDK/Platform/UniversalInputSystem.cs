using UnityEngine;
using UnityEngine.InputSystem;

namespace LTSystem.Core
{
    /// <summary>
    /// Unified input abstraction that routes actions to platform-appropriate
    /// input handlers. Consumers should subscribe to events, not poll directly.
    /// Uses Unity's new Input System (2023+).
    /// </summary>
    public class UniversalInputSystem : MonoBehaviour
    {
        public static UniversalInputSystem Instance { get; private set; }

        [Header("Input Actions Asset")]
        [Tooltip("Assign the LoveTriggerInputActions asset here.")]
        [SerializeField] private InputActionAsset _inputActions;

        // ── Events ────────────────────────────────
        public System.Action OnInteractPressed;
        public System.Action OnMenuPressed;
        public System.Action OnConfirmPressed;
        public System.Action OnCancelPressed;

        // ── Internal ──────────────────────────────
        private InputAction _interactAction;
        private InputAction _menuAction;
        private InputAction _confirmAction;
        private InputAction _cancelAction;

        // ── Lifecycle ─────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BindActions();
        }

        private void OnDestroy() => UnbindActions();

        // ── Internal ──────────────────────────────
        private void BindActions()
        {
            if (_inputActions == null)
            {
                Debug.LogWarning("[UniversalInputSystem] No InputActionAsset assigned — input will not function.");
                return;
            }

            _interactAction = _inputActions.FindAction("Interact");
            _menuAction     = _inputActions.FindAction("Menu");
            _confirmAction  = _inputActions.FindAction("Confirm");
            _cancelAction   = _inputActions.FindAction("Cancel");

            if (_interactAction != null) _interactAction.performed += _ => OnInteractPressed?.Invoke();
            if (_menuAction     != null) _menuAction.performed     += _ => OnMenuPressed?.Invoke();
            if (_confirmAction  != null) _confirmAction.performed  += _ => OnConfirmPressed?.Invoke();
            if (_cancelAction   != null) _cancelAction.performed   += _ => OnCancelPressed?.Invoke();

            _inputActions.Enable();
            Debug.Log($"[UniversalInputSystem] Inputs bound for platform: {PlatformManager.Current}");
        }

        private void UnbindActions()
        {
            _inputActions?.Disable();
        }
    }
}
