using UnityEngine;

namespace HexaBit.Core {
    // Local high-score storage using PlayerPrefs.
    // API mirrors Apple.GameKit's GKLeaderboard so it can be swapped later.
    public static class LocalLeaderboardService {
        private const string KeyPrefix = "HS_";

        // Stores the score only if it beats the current high score.
        // Mirrors GKLeaderboard.SubmitScore's "best score wins" behavior.
        public static void SubmitScore(string leaderboardID, long score) {
            if (string.IsNullOrEmpty(leaderboardID)) return;

            long current = GetHighScore(leaderboardID);
            if (score > current) {
                PlayerPrefs.SetString(KeyPrefix + leaderboardID, score.ToString());
                PlayerPrefs.Save();
                Debug.Log($"[LocalLeaderboard] New high score for '{leaderboardID}': {score}");
            }
        }

        // Returns the stored high score for a leaderboard, or 0 if none.
        public static long GetHighScore(string leaderboardID) {
            if (string.IsNullOrEmpty(leaderboardID)) return 0;
            string raw = PlayerPrefs.GetString(KeyPrefix + leaderboardID, "0");
            return long.TryParse(raw, out long value) ? value : 0;
        }

        // Wipes a leaderboard entry (useful for debug / dev builds).
        public static void Reset(string leaderboardID) {
            if (string.IsNullOrEmpty(leaderboardID)) return;
            PlayerPrefs.DeleteKey(KeyPrefix + leaderboardID);
            PlayerPrefs.Save();
        }
    }
}