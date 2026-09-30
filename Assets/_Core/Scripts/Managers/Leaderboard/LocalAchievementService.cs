using UnityEngine;

namespace HexaBit.Core {
    /// <summary>
    /// Local achievement storage using PlayerPrefs.
    /// API mirrors Apple.GameKit's GKAchievement so it can be swapped later
    /// with a single-line change in AchievementManager.
    /// </summary>
    public static class LocalAchievementService {
        private const string KeyPrefix = "ACH_";

        /// <summary>
        /// Reports the completion percentage (0..1) for an achievement.
        /// Only raises the value — never lowers it — matching GKAchievement's
        /// monotonic progress behavior.
        /// </summary>
        public static void ReportProgress(string id, float percent) {
            if (string.IsNullOrEmpty(id)) return;
            percent = Mathf.Clamp01(percent);

            float current = GetProgress(id);
            if (percent <= current) return;

            PlayerPrefs.SetFloat(KeyPrefix + id, percent);
            PlayerPrefs.Save();

            Debug.Log($"[Achievement] '{id}' => {percent * 100f:F1}%");

            if (percent >= 1f) {
                Debug.Log($"[Achievement] 🏆 UNLOCKED: '{id}'");
                // GameKit would show its native completion banner here.
                AchievementManager.NotifyUnlocked(id);
            }
        }

        public static float GetProgress(string id) {
            if (string.IsNullOrEmpty(id)) return 0f;
            return PlayerPrefs.GetFloat(KeyPrefix + id, 0f);
        }

        public static bool IsUnlocked(string id) => GetProgress(id) >= 1f;

        public static void Reset(string id) {
            if (string.IsNullOrEmpty(id)) return;
            PlayerPrefs.DeleteKey(KeyPrefix + id);
            PlayerPrefs.Save();
        }
    }
}