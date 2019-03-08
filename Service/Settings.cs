using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Service {
    public class Settings {
        public const string DiscordAppId = "551089446460850176";
        public const string ProgramName = "Exiled Presence";
        public const string GameWindowTitle = "Path of Exile";
        public const string ConfigFileName = "config.json";
        public const string Version = "v1.0.1";
        public const int CharacterUpdateDelaySec = 60;
        public const int PresencePollDelayMs = 500;
        public const int UpdateCheckIntervalH = 24;

        public string AccountName { get; set; } = "";
        public string PoeSessionId { get; set; } = "";
        public DateTime? LastUpdateCheck { get; set; }

        /// <summary>
        /// Loads in settings from another instance
        /// </summary>
        public void Update(Settings settings) {
            AccountName = settings.AccountName;
            PoeSessionId = settings.PoeSessionId;
            LastUpdateCheck = settings.LastUpdateCheck;
        }

        public bool Validate(out string errorMsg) {
            if (!string.IsNullOrEmpty(PoeSessionId) && !Config.SessIdRegex.IsMatch(PoeSessionId)) {
                errorMsg = "Invalid session ID";
                return false;
            }

            if (!string.IsNullOrEmpty(AccountName) && AccountName.Length < 3) {
                errorMsg = "Invalid account name";
                return false;
            }

            errorMsg = null;
            return true;
        }

        /// <summary>
        /// Compares time in config to current time
        /// </summary>
        public bool IsCheckUpdates() {
            if (LastUpdateCheck == null) {
                return true;
            }

            return LastUpdateCheck.Value.AddHours(UpdateCheckIntervalH) < DateTime.UtcNow;
        }
    }
}