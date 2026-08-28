using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace HexaBit.Core {
    public class HUDController : MonoBehaviour {
        [Header("XP Bar")]
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI killsText;

        [SerializeField] private LocalizedString levelFormat = new LocalizedString("UI_Texts", "hud_level");
        [SerializeField] private LocalizedString killsFormat = new LocalizedString("UI_Texts", "hud_kills");

        private int currentLevel;
        private int currentKills;

        private void Start() {
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.OnXPChanged.AddListener(UpdateXP);
                GameplayManager.Instance.OnLevelUp.AddListener(UpdateLevel);
                GameplayManager.Instance.OnKillCountChanged.AddListener(UpdateKills);
            }

            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            currentLevel = GameplayManager.Instance.CurrentLevel;
            currentKills = GameplayManager.Instance.TotalKills;
            UpdateXP(GameplayManager.Instance.CurrentXP);
            RefreshTexts();
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale) {
            RefreshTexts();
        }

        private void RefreshTexts() {
            if (levelText != null)
                levelText.text = levelFormat.GetLocalizedString(currentLevel);
            if (killsText != null)
                killsText.text = killsFormat.GetLocalizedString(currentKills);
        }

        private void UpdateXP(int currentXP) {
            if (xpSlider != null) {
                float progress = (float)currentXP / GameplayManager.Instance.XPToNextLevel;
                xpSlider.value = Mathf.Clamp01(progress);
            }
        }

        private void UpdateLevel(int level) {
            currentLevel = level;
            RefreshTexts();
            if (xpSlider != null) xpSlider.value = 0;
        }

        private void UpdateKills(int kills) {
            currentKills = kills;
            RefreshTexts();
        }

        private void OnDestroy() {
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.OnXPChanged.RemoveListener(UpdateXP);
                GameplayManager.Instance.OnLevelUp.RemoveListener(UpdateLevel);
                GameplayManager.Instance.OnKillCountChanged.RemoveListener(UpdateKills);
            }
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
    }
}