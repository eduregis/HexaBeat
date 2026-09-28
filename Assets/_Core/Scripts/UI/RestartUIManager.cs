using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HexaBit.Core {
    public class RestartUIManager : MonoBehaviour {
        public static RestartUIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject restartPanel;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;
        private HUDButton[] _buttons;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _buttons = new HUDButton[] { restartButton, quitButton };

            if (restartPanel != null)
                restartPanel.SetActive(false);

            // Setup button listeners
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

        /// <summary>
        /// Opens the restart menu. Does NOT freeze time - the arena stays visible.
        /// The hero cannot move because IsDead is true.
        /// </summary>
        public void OpenRestartMenu() {
            if (_isOpen) return;

            _isOpen = true;

            // Pause only the timer (survival time stops at death), but not the game itself
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(true);
            }

            // 🔥 Do NOT freeze time - arena remains visible and animated
            if (restartPanel != null)
                restartPanel.SetActive(true);

            if (restartButton != null && restartButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(restartButton.button.gameObject);
                SetSelectedButton(0);
            }
        }

        /// <summary>
        /// Closes the restart menu without restarting (not normally used).
        /// </summary>
        public void CloseRestartMenu() {
            if (!_isOpen) return;

            _isOpen = false;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            if (restartPanel != null)
                restartPanel.SetActive(false);

            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Reloads the current scene, restarting the run.
        /// </summary>
        public void RestartGame() {
            // GameplayManager resets the timer state on scene reload
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            if (restartPanel != null)
                restartPanel.SetActive(false);

            _isOpen = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Quit action. Currently a no-op placeholder for future navigation.
        /// </summary>
        public void QuitGame() {
            Debug.Log("RestartUIManager: QuitGame called (no-op for now)");
            // TODO: Implement future navigation (e.g., return to main menu)
        }

        private void OnDestroy() {
            if (Instance == this) Instance = null;
        }
    }
}