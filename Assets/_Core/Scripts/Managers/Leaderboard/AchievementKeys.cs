namespace HexaBit.Core {
    /// <summary>
    /// Achievement identifiers shared between the local fallback
    /// and the eventual Apple GameKit implementation.
    /// Must match the IDs registered in App Store Connect.
    /// </summary>
    public static class AchievementKeys {
        public const string FirstBlood   = "hexabeat.ach.first_blood";    // 1 kill
        public const string Kills100     = "hexabeat.ach.kills_100";      // 100 kills in a single run
        public const string Survive10Min = "hexabeat.ach.survive_10min";  // survive 10 minutes
    }
}