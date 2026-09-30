using UnityEngine;

namespace HexaBit.Core {
    /// <summary>
    /// Centralized leaderboard identifiers, shared between the local fallback
    /// and the eventual Apple GameKit implementation.
    /// </summary>
    public static class LeaderboardKeys {
        public const string SurvivalTime = "hexabeat.survival.time";
        public const string TotalKills   = "hexabeat.total.kills";
    }
}
