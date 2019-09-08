using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Domain {
    /// <summary>
    /// Contains application-wide constants and settings
    /// </summary>
    public class Settings {
        public const string DiscordAppId = "551089446460850176";
        public const string ProgramName = "Exiled Presence";
        public const string GameWindowTitle = "Path of Exile";
        public const string Version = "v1.0.5";

        public const string ReleaseApi = "https://api.github.com/repos/siegrest/Exiled-Presence/releases";
        public const string CharApi = "https://www.pathofexile.com/character-window/get-characters";

        public static readonly string StartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static readonly string StartupShortcutPath = Path.Combine(StartupFolderPath, $"{ProgramName}.url");
        public static readonly string AppPath = Assembly.GetEntryAssembly().Location;

        public static readonly TimeSpan PresencePollInterval = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromMinutes(60);
        public static readonly TimeSpan CharacterUpdateInterval = TimeSpan.FromSeconds(60);
        private const string ConfTimeFormat = "yyyy-MM-dd HH:mm";

        private readonly Dictionary<SettingType, string> _defaultSettings = new Dictionary<SettingType,string> {
            {SettingType.AccountName, null},
            {SettingType.PoeSessionId, null},
            {SettingType.ShowCharName, "true"},
            {SettingType.ShowCharXp, "true"},
            {SettingType.ShowCharLevel, "true"},
            {SettingType.ShowElapsedTime, "true"},
            {SettingType.PersistentTimer, "true"},
            {SettingType.LastUpdateCheck, DateTime.MinValue.ToString(ConfTimeFormat)},
            {SettingType.DiscordPipe, "-1"},
            {SettingType.ConfigVersion, Version}
        };

        private readonly Dictionary<SettingType, string> _userSettings = new Dictionary<SettingType, string>();

        /// <summary>
        /// Loads the default values
        /// </summary>
        public void Reset() {
            _userSettings.Clear();
        }

        /// <summary>
        /// Attempt to store value with key
        /// </summary>
        public void ParseValue(string key, string val) {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Empty key provided");
            }

            // Ignored value
            if (val.StartsWith("#")) {
                val = null;
            }

            // Convert string key to enum
            SettingType type;
            try {
                type = SettingMethods.GetType(key);
            } catch (InvalidOperationException) {
                return;
            }

            _userSettings[type] = val;

            // Validate user input
            type.Validate(val);
            Console.WriteLine($"[CONF] {type} - {val}");
        }

        /// <summary>
        /// If user settings has same nr of settings as default settings
        /// </summary>
        public void VerifyAllSettingsPresent() {
            // Check if all mandatory config values are present
            foreach (SettingType type in Enum.GetValues(typeof(SettingType))) {
                if (!_userSettings.ContainsKey(type)) {
                    throw new ArgumentException($"Missing config option '{SettingMethods.GetKey(type)}'. " +
                                                "Please backup and reset the config");
                }
            }
        }

        /// <summary>
        /// Sets the last update check time to now
        /// </summary>
        public void UpdateLastUpdateTime() {
            _userSettings[SettingType.LastUpdateCheck] = DateTime.Now.ToString(ConfTimeFormat);
        }

        public bool IsCheckUpdates() {
            var date = GetDateTime(SettingType.LastUpdateCheck);
            return DateTime.Equals(date, DateTime.MinValue) || date < DateTime.Now.Subtract(UpdateCheckInterval);
        }



        /// <summary>
        /// Get first setting matching the type
        /// </summary>
        /// <param name="settingType"></param>
        /// <returns></returns>
        private string GetValue(SettingType settingType) {
            if (_userSettings.TryGetValue(settingType, out var val)) {
                return val;
            } 
            
            if (_defaultSettings.TryGetValue(settingType, out val)) {
                return val;
            }
            
            throw new Exception($"Cannot find value for '{settingType}'");
        }



        public string GetString(SettingType settingType) {
            return GetValue(settingType);
        }

        public bool GetBool(SettingType settingType) {
            var setting = GetValue(settingType);
            
            // Can we parse it as bool?
            if (!bool.TryParse(setting, out var parsedVal)) {
                throw new ArgumentException($"Cannot parse value '{setting}' as bool");
            }
            
            return parsedVal;
        }

        public int GetInt(SettingType settingType) {
            var setting = GetValue(settingType);
            
            // Can we parse it as int?
            if (!int.TryParse(setting, out var parsedVal)) {
                throw new ArgumentException($"Cannot parse value '{setting}' as int");
            }
            
            return parsedVal;
        }

        public double GetDouble(SettingType settingType) {
            var setting = GetValue(settingType);
            
            // Can we parse it as double?
            if (!double.TryParse(setting, out var parsedVal)) {
                throw new ArgumentException($"Cannot parse value '{setting}' as double");
            }
            
            return parsedVal;
        }

        public DateTime GetDateTime(SettingType settingType) {
            var setting = GetValue(settingType);
            
            // Can we parse it as DateTime?
            if (!DateTime.TryParseExact(setting, ConfTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var parsedVal)) {
                throw new ArgumentException($"Cannot parse value '{setting}' as DateTime");
            }
            
            return parsedVal;
        }
    }
}