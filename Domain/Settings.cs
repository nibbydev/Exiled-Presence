using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Domain {
    public class Settings {
        public const string DiscordAppId = "551089446460850176";
        public const string ProgramName = "Exiled Presence";
        public const string GameWindowTitle = "Path of Exile";
        public const string Version = "v1.0.3";

        public static readonly string StartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static readonly string StartupShortcutPath = Path.Combine(StartupFolderPath, $"{ProgramName}.url");
        public static readonly string AppPath = Assembly.GetEntryAssembly().Location;
        public static readonly Regex SessIdRegex = new Regex("^[0-9a-fA-F]{32}$");

        public static readonly TimeSpan PresencePollInterval = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);
        public static readonly TimeSpan CharacterUpdateInterval = TimeSpan.FromSeconds(60);
        private const string ConfTimeFormat = "yyyy-MM-dd HH:mm";

        public string AccountName { get; set; }
        public string PoeSessionId { get; set; }
        public string LastUpdateCheck { get; set; }

        /// <summary>
        /// Loads in settings from another instance
        /// </summary>
        public void Update(Settings settings) {
            AccountName = settings.AccountName;
            PoeSessionId = settings.PoeSessionId;
            LastUpdateCheck = settings.LastUpdateCheck;
        }

        /// <summary>
        /// Validate all config options
        /// </summary>
        public void Validate() {
            if (!string.IsNullOrEmpty(PoeSessionId) && !SessIdRegex.IsMatch(PoeSessionId)) {
                throw new ArgumentException("Invalid session ID");
            }

            if (!string.IsNullOrEmpty(AccountName) && AccountName.Length < 3) {
                throw new ArgumentException("Invalid account name");
            }

            if (!string.IsNullOrEmpty(LastUpdateCheck) && !DateTime.TryParseExact(LastUpdateCheck, ConfTimeFormat, 
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _)) {
                throw new ArgumentException("Invalid last update time");
            }
        }

        /// <summary>
        /// Compares time in config to current time to determine if updates should be checked
        /// </summary>
        public bool IsCheckUpdates() {
            if (LastUpdateCheck == null) {
                return true;
            }

            return DateTime.Parse(LastUpdateCheck) < DateTime.UtcNow.Subtract(UpdateCheckInterval);
        }

        /// <summary>
        /// Loads the default values
        /// </summary>
        public void Reset() {
            Update(new Settings());
        }

        /// <summary>
        /// Attempt to store value with key
        /// </summary>
        public void ParseValue(string key, string val) {
            switch (key) {
                case "account name":
                    AccountName = val;
                    break;

                case "POESESSID":
                    PoeSessionId = val;
                    break;

                case "last update check":
                    LastUpdateCheck = val;
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        /// Attempt to get value from key
        /// </summary>
        public string GetValue(string key) {
            switch (key) {
                case "account name":
                    return AccountName;

                case "POESESSID":
                    return PoeSessionId;

                case "last update check":
                    return LastUpdateCheck;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the last update check time to now
        /// </summary>
        public void UpdateLastUpdateTime() {
            LastUpdateCheck = DateTime.UtcNow.ToString(ConfTimeFormat);
        }
    }
}