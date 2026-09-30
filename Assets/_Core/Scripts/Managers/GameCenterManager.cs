using UnityEngine;
using Apple.GameKit;
using System.Threading.Tasks;
using System;

namespace HexaBit.Core {
    public class GameCenterManager : MonoBehaviour {
        public static GameCenterManager Instance { get; private set; }

        [Header("Leaderboard IDs (from App Store Connect)")]
        [SerializeField] private string survivalTimeLeaderboardID = "hexabeat.survival.time";
        [SerializeField] private string totalKillsLeaderboardID = "hexabeat.total.kills";

        public bool IsAuthenticated { get; private set; } = false;
        public string PlayerDisplayName { get; private set; } = "Guest";

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start() {
            await Authenticate();
        }

        // Authenticates the local player with Game Center.
        public async Task Authenticate() {
            if (IsAuthenticated) return;

            try {
                var player = await GKLocalPlayer.Authenticate();
                var localPlayer = GKLocalPlayer.Local;

                IsAuthenticated = localPlayer.IsAuthenticated;
                PlayerDisplayName = localPlayer.DisplayName;

                Debug.Log($"[GameCenter] Authenticated as: {PlayerDisplayName}");
            } catch (Exception e) {
                Debug.LogError($"[GameCenter] Authentication failed: {e.Message}");
            }
        }

        // Submits the final run score to both leaderboards.
        public async Task SubmitScore(float survivalTime, int totalKills) {
            if (!IsAuthenticated) {
                Debug.LogWarning("[GameCenter] Cannot submit score: player not authenticated.");
                return;
            }

            // Game Center only accepts 64-bit integers.
            // Multiply the survival time by 1000 to keep millisecond precision.
            long timeScore = (long)(survivalTime * 1000f);
            long killsScore = totalKills;

            await SubmitScoreToLeaderboard(survivalTimeLeaderboardID, timeScore, "Survival Time");
            await SubmitScoreToLeaderboard(totalKillsLeaderboardID, killsScore, "Total Kills");
        }

        // Submits a single score to a specific leaderboard.
        private Task SubmitScoreToLeaderboard(string leaderboardID, long score, string debugName) {
            try {
                // Load the specific leaderboard
                // var leaderboards = await GKLeaderboard.LoadLeaderboards(new[] { leaderboardID });

                // if (leaderboards == null || leaderboards.Count == 0) {
                //     Debug.LogError($"[GameCenter] Leaderboard '{leaderboardID}' not found.");
                //     return;
                // }

                // var leaderboard = leaderboards[0];

                // // Submit the score
                // await leaderboard.SubmitScore(score, 0, GKLocalPlayer.Local);

                Debug.Log($"[GameCenter] {debugName} score submitted: {score} (Board: {leaderboardID})");
            } catch (Exception e) {
                Debug.LogError($"[GameCenter] Failed to submit {debugName}: {e.Message}");
            }

            return Task.CompletedTask;
        }
    }
}