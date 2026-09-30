using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace HexaBit.Core {
    public class GameOverUIManager : MonoBehaviour {
        public static GameOverUIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject restartPanel;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        [Header("Score Labels")]
        [SerializeField] private TextMeshProUGUI killsText;      // Kills desta run
        [SerializeField] private TextMeshProUGUI highScoreText;  // Melhor kills local

        [Header("Localization")]
        [SerializeField] private LocalizedString killsFormat     = new LocalizedString("UI_Texts", "hud_kills");
        [SerializeField] private LocalizedString highScoreFormat = new LocalizedString("UI_Texts", "hud_highscore");

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;
        private HUDButton[] _buttons;

        // Guardamos os valores para reformatar quando o idioma mudar
        private int _lastKills;
        private long _lastHighScore;

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

        private void OnEnable() {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable() {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale) {
            // Reformata os textos se o menu estiver aberto
            if (_isOpen) {
                RefreshScoreTexts();
            }
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
        /// Abre o menu de game over com as estatísticas da run.
        /// Salva os scores localmente e atualiza os textos localizados.
        /// </summary>
        public void OpenGameOverMenu(int totalKills, float survivalTime) {
            if (_isOpen) return;
            _isOpen = true;

            // --- 1. Persistir scores (local por enquanto) ---
            long timeScore = (long)(survivalTime * 1000f);
            long killsScore = totalKills;

            LocalLeaderboardService.SubmitScore(LeaderboardKeys.SurvivalTime, timeScore);
            LocalLeaderboardService.SubmitScore(LeaderboardKeys.TotalKills,   killsScore);

            // --- 2. Guardar valores e atualizar textos ---
            _lastKills = totalKills;
            _lastHighScore = LocalLeaderboardService.GetHighScore(LeaderboardKeys.TotalKills);
            RefreshScoreTexts();

            // --- 3. Lógica existente do menu ---
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetTimerPaused(true);

            if (restartPanel != null)
                restartPanel.SetActive(true);

            if (restartButton != null && restartButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(restartButton.button.gameObject);
                SetSelectedButton(0);
            }
        }

        /// <summary>
        /// Overload de compatibilidade para chamadas sem estatísticas.
        /// </summary>
        public void OpenGameOverMenu() {
            OpenGameOverMenu(0, 0f);
        }

        private void RefreshScoreTexts() {
            if (killsText != null && killsFormat != null) {
                killsText.text = killsFormat.GetLocalizedString(_lastKills);
            }
            if (highScoreText != null && highScoreFormat != null) {
                highScoreText.text = highScoreFormat.GetLocalizedString(_lastHighScore);
            }
        }

        public void CloseRestartMenu() {
            if (!_isOpen) return;
            _isOpen = false;

            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetTimerPaused(false);

            if (restartPanel != null)
                restartPanel.SetActive(false);

            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void RestartGame() {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.SetTimerPaused(false);

            if (restartPanel != null)
                restartPanel.SetActive(false);

            _isOpen = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame() {
            Debug.Log("GameOverUIManager: QuitGame called (no-op for now)");
            // TODO: Implement future navigation (e.g., return to main menu)
        }

        private void OnDestroy() {
            if (Instance == this) Instance = null;
        }
    }
}