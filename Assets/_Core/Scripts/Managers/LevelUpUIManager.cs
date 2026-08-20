using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Required for TextMeshPro

namespace HexaBit.Core {
    public class LevelUpUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private Button[] choiceButtons;

        private System.Action<int> _onChoiceMade;

        public void OpenWithOptions(System.Collections.Generic.List<GameplayManager.LevelUpOption> options, System.Action<int> onChoiceMade) {
            _onChoiceMade = onChoiceMade;

            Time.timeScale = 0f;
            levelUpPanel.SetActive(true);

            for (int i = 0; i < choiceButtons.Length && i < options.Count; i++) {
                Button btn = choiceButtons[i];
                GameplayManager.LevelUpOption opt = options[i];

                TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) {
                    btnText.text = opt.displayName;
                }

                btn.onClick.RemoveAllListeners();
                int capturedIndex = i;
                btn.onClick.AddListener(() => {
                    _onChoiceMade?.Invoke(capturedIndex);
                    CloseLevelUpMenu();
                });
            }

            if (choiceButtons.Length > 0 && choiceButtons[0] != null) {
                EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
            }
        }

        public void CloseLevelUpMenu() {
            Time.timeScale = 1f;
            Destroy(gameObject);
        }

        private void OnDestroy() {
            Time.timeScale = 1f;
        }
    }
}