using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Service {
    public class Settings {
        public const string DiscordAppId = "551089446460850176";
        public const string ProgramName = "Exiled Presence";
        public const string GameWindowTitle = "Path of Exile";
        public const string Version = "v1.0.2";
        public const int CharacterUpdateDelaySec = 60;
        public const int UpdateCheckIntervalH = 24;
        
        public const string ConfigFileName = "config.json";
        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string CfgFolderPath = Path.Combine(AppDataPath, ProgramName);
        public static readonly string CfgFilePath = Path.Combine(CfgFolderPath, ConfigFileName);
        public static readonly string StartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static readonly string StartupShortcutPath =  Path.Combine(StartupFolderPath, $"{ProgramName}.url");
        public static readonly string AppPath = Assembly.GetEntryAssembly().Location;
        public static readonly Regex SessIdRegex = new Regex("^[0-9a-fA-F]{32}$");
        public static readonly TimeSpan PresencePollDelay = TimeSpan.FromMilliseconds(500);

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
            if (!string.IsNullOrEmpty(PoeSessionId) && !SessIdRegex.IsMatch(PoeSessionId)) {
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