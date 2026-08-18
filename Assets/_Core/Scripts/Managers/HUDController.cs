using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {
    [Header("XP Bar")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI killsText; // Opcional

    private void Start() {
        if (GameplayManager.Instance != null) {
            GameplayManager.Instance.OnXPChanged.AddListener(UpdateXP);
            GameplayManager.Instance.OnLevelUp.AddListener(UpdateLevel);
            GameplayManager.Instance.OnKillCountChanged.AddListener(UpdateKills);
        }

        // Initial update
        UpdateXP(GameplayManager.Instance.CurrentXP);
        UpdateLevel(GameplayManager.Instance.CurrentLevel);
        UpdateKills(GameplayManager.Instance.TotalKills);
    }

    private void UpdateXP(int currentXP) {
        if (xpSlider != null) {
            float progress = (float)currentXP / GameplayManager.Instance.XPToNextLevel;
            xpSlider.value = Mathf.Clamp01(progress);
        }
    }

    private void UpdateLevel(int level) {
        if (levelText != null) levelText.text = $"Lv. {level}";
        xpSlider.value = 0;
    }

    private void UpdateKills(int kills) {
        if (killsText != null) killsText.text = $"Kills: {kills}";
    }

    private void OnDestroy() {
        if (GameplayManager.Instance != null) {
            GameplayManager.Instance.OnXPChanged.RemoveListener(UpdateXP);
            GameplayManager.Instance.OnLevelUp.RemoveListener(UpdateLevel);
            GameplayManager.Instance.OnKillCountChanged.RemoveListener(UpdateKills);
        }
    }
}