using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaBit.Core {
    public class PauseInputHandler : MonoBehaviour {
        private PlayerInput playerInput;
        private InputAction pauseAction;

        private void Awake() {
            playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable() {
            if (playerInput != null && playerInput.actions != null) {
                pauseAction = playerInput.actions["Pause"];
                if (pauseAction != null) {
                    pauseAction.performed += OnPausePerformed;
                } else {
                    Debug.LogWarning("PauseInputHandler: 'Pause' action not found in PlayerInput actions!");
                }
            }
        }

        private void OnDisable() {
            if (pauseAction != null) {
                pauseAction.performed -= OnPausePerformed;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context) {
            if (PauseUIManager.Instance == null) {
                Debug.LogWarning("PauseInputHandler: PauseUIManager.Instance is null!");
                return;
            }

            // If time is frozen but pause menu isn't open, another menu (LevelUp, Restart) is active.
            // Ignore the pause input in this case.
            if (Time.timeScale == 0f && !PauseUIManager.Instance.IsPaused) {
                return;
            }

            // Toggle: if paused, resume; otherwise, open pause menu
            if (PauseUIManager.Instance.IsPaused) {
                PauseUIManager.Instance.ResumeGame();
            } else {
                PauseUIManager.Instance.OpenPauseMenu();
            }
        }
    }
}