using UnityEngine;
using System;

namespace HexaBit.Core {
    /// <summary>
    /// Listens to gameplay events and reports achievement progress.
    /// Right now it talks to LocalAchievementService; to migrate to GameKit,
    /// replace those calls with GameCenterManager.Instance.ReportAchievement(...).
    /// </summary>
    public class AchievementManager : MonoBehaviour {
        public static AchievementManager Instance { get; private set; }

        /// <summary>
        /// Fired whenever an achievement is unlocked. UI can subscribe to show a toast.
        /// When GameKit is active, this won't be needed (native banner handles it).
        /// </summary>
        public static event Action<string> OnAchievementUnlocked;

        [Header("Thresholds")]
        [SerializeField] private int killsForAchievement = 100;
        [SerializeField] private float secondsForSurvival = 600f; // 10 min

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() {
            if (GameplayManager.Instance != null) Subscribe();
        }

        private void Start() {
            // Failsafe in case GameplayManager.Instance wasn't ready during OnEnable.
            Subscribe();
        }

        private void OnDisable() {
            Unsubscribe();
        }

        private void Subscribe() {
            if (GameplayManager.Instance == null) return;
            GameplayManager.Instance.OnKillCountChanged.AddListener(CheckKills);
            GameplayManager.Instance.OnTimerUpdated.AddListener(CheckSurvival);
        }

        private void Unsubscribe() {
            if (GameplayManager.Instance == null) return;
            GameplayManager.Instance.OnKillCountChanged.RemoveListener(CheckKills);
            GameplayManager.Instance.OnTimerUpdated.RemoveListener(CheckSurvival);
        }

        // --- Checks ---

        private void CheckKills(int kills) {
            if (kills >= 1) {
                Unlock(AchievementKeys.FirstBlood);
            }

            if (kills >= killsForAchievement) {
                Unlock(AchievementKeys.Kills100);
            }
        }

        private void CheckSurvival(float time) {
            if (time >= secondsForSurvival) {
                Unlock(AchievementKeys.Survive10Min);
            }
        }

        // --- Reporting (swap target for GameKit) ---

        /// <summary>
        /// Unlocks an achievement (100%).
        /// To migrate to GameKit, replace the LocalAchievementService call
        /// with: await GameCenterManager.Instance.ReportAchievement(id, 1f);
        /// </summary>
        private void Unlock(string id) {
            if (LocalAchievementService.IsUnlocked(id)) return;
            LocalAchievementService.ReportProgress(id, 1f);
        }

        /// <summary>
        /// Reports partial progress (for future progressive achievements).
        /// </summary>
        private void ReportProgress(string id, float percent) {
            LocalAchievementService.ReportProgress(id, percent);
        }

        // --- Internal notification bridge ---

        internal static void NotifyUnlocked(string id) {
            OnAchievementUnlocked?.Invoke(id);
        }
    }
}