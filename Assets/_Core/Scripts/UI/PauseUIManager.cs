using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HexaBit.Core {
    public class PauseUIManager : MonoBehaviour {
        public static PauseUIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private HUDButton resumeButton;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        private bool _isPaused = false;
        public bool IsPaused => _isPaused;
        private HUDButton[] _buttons;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _buttons = new HUDButton[] { resumeButton, restartButton, quitButton };

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

            SetupSelectionListeners();
        }

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

        private void SetSelectedButton(int index) {
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i] != null) {
                    _buttons[i].SetSelected(i == index);
                }
            }
        }

        public void OpenPauseMenu() {
            if (_isPaused) return;

            _isPaused = true;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(true);
            }

            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);

            if (resumeButton != null && resumeButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(resumeButton.button.gameObject);
                SetSelectedButton(0);
            }
        }

        public void ResumeGame() {
            if (!_isPaused) return;

            _isPaused = false;

            Time.timeScale = 1f;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void RestartGame() {
            Time.timeScale = 1f;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            // Hide the pause panel before reloading
            if (pausePanel != null)
                pausePanel.SetActive(false);

            _isPaused = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Quit action. Currently a no-op placeholder for future navigation.
        /// </summary>
        public void QuitGame() {
            Debug.Log("PauseUIManager: QuitGame called (no-op for now)");
            // TODO: Implement future navigation (e.g., return to main menu)
        }

        private void OnDestroy() {
            Time.timeScale = 1f;
            if (Instance == this) Instance = null;
        }
    }
}