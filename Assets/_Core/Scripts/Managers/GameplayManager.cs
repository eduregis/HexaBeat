using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour {
    public static GameplayManager Instance { get; private set; }

    [Header("XP Settings")]
    [SerializeField] private int baseXPToLevel = 20;
    [SerializeField] private int xpPerLevelMultiplier = 15;

    [Header("Runtime Stats")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalKills = 0;

    [Header("Level Up UI")]
    [SerializeField] private GameObject levelUpPanelPrefab;

    public int CurrentXP => currentXP;
    public int CurrentLevel => currentLevel;
    public int TotalKills => totalKills;
    public int XPToNextLevel => baseXPToLevel + (currentLevel - 1) * xpPerLevelMultiplier;

    // Events
    public UnityEvent<int> OnXPChanged;
    public UnityEvent<int> OnLevelUp;
    public UnityEvent<int> OnKillCountChanged;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddXP(int amount) {
        currentXP += amount;
        OnXPChanged?.Invoke(currentXP);

        while (currentXP >= XPToNextLevel) {
            currentXP -= XPToNextLevel;
            currentLevel++;

            if (levelUpPanelPrefab != null) {
                Instantiate(levelUpPanelPrefab);
            }

            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"Level Up! Now Level {currentLevel}");
        }
    }

    public void AddKill() {
        totalKills++;
        OnKillCountChanged?.Invoke(totalKills);
    }
}