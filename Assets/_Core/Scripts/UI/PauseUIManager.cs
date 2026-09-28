using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HexaBit.Core {
    public class PauseUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private HUDButton resumeButton;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        private bool _isPaused = false;
        private HUDButton[] _buttons;

        private void Awake() {
            // Collect all buttons for navigation
            _buttons = new HUDButton[] { resumeButton, restartButton, quitButton };

            // Ensure the panel starts hidden
            if (pausePanel != null)
                pausePanel.SetActive(false);

            // Setup button listeners
            if (resumeButton != null && resumeButton.button != null) {
                resumeButton.button.onClick.RemoveAllListeners();
                resumeButton.button.onClick.AddListener(ResumeGame);
            }

            if (restartButton != null && restartButton.button != null) {
                restartButton.button.onClick.RemoveAllListeners();
                restartButton.button.onClick.AddListener(RestartGame);
            }

            if (quitButton != null && quitButton.button != null) {
                quitButton.button.onClick.RemoveAllListeners();
                quitButton.button.onClick.AddListener(QuitGame);
            }

            // Setup selection listeners for glow feedback
            SetupSelectionListeners();
        }

        /// <summary>
        /// Sets up EventTrigger listeners to detect when each button is selected.
        /// </summary>
        private void SetupSelectionListeners() {
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i] == null || _buttons[i].button == null) continue;

                EventTrigger trigger = _buttons[i].button.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = _buttons[i].button.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry();
                selectEntry.eventID = EventTriggerType.Select;
                int capturedIndex = i;
                selectEntry.callback.AddListener((data) => {
                    SetSelectedButton(capturedIndex);
                });
                trigger.triggers.Add(selectEntry);
            }
        }

        /// <summary>
        /// Highlights the selected button and removes highlight from others.
        /// </summary>
        private void SetSelectedButton(int index) {
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i] != null) {
                    _buttons[i].SetSelected(i == index);
                }
            }
        }

        /// <summary>
        /// Opens the pause menu, freezing the game.
        /// </summary>
        public void OpenPauseMenu() {
            if (_isPaused) return;

            _isPaused = true;

            // Pause the game timer
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(true);
            }

            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);

            // Select the resume button by default
            if (resumeButton != null && resumeButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(resumeButton.button.gameObject);
                SetSelectedButton(0);
            }
        }

        /// <summary>
        /// Closes the pause menu, resuming the game.
        /// </summary>
        public void ResumeGame() {
            if (!_isPaused) return;

            _isPaused = false;

            Time.timeScale = 1f;

            // Resume the game timer
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            if (pausePanel != null)
                pausePanel.SetActive(false);

            // Clear selection
            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Restarts the current scene.
        /// </summary>
        public void RestartGame() {
            Time.timeScale = 1f;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Quits the game (or returns to main menu).
        /// </summary>
        public void QuitGame() {
            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        private void OnDestroy() {
            Time.timeScale = 1f;
        }
    }
}