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
        public static readonly Regex VersionRegex = new Regex(@"^v\d+(\.\d+)*$");

        public static readonly TimeSpan PresencePollInterval = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);
        public static readonly TimeSpan CharacterUpdateInterval = TimeSpan.FromSeconds(60);
        private const string ConfTimeFormat = "yyyy-MM-dd HH:mm";


        public string AccountName { get; set; }
        public string PoeSessionId { get; set; }

        private string _configVersion;
        private string _lastUpdateCheck;
        private string _showElapsedTime;
        private string _showCharName;
        private string _showCharXp;
        private string _showCharLevel;

        public bool ShowElapsedTime => _showElapsedTime.Equals("1");
        public bool ShowCharName => _showCharName.Equals("1");
        public bool ShowCharXp => _showCharXp.Equals("1");
        public bool ShowCharLevel => _showCharLevel.Equals("1");

        public bool CheckUpdates => _lastUpdateCheck == null ||
                                    DateTime.Parse(_lastUpdateCheck) <
                                    DateTime.UtcNow.Subtract(UpdateCheckInterval);

        /// <summary>
        /// Loads in settings from another instance
        /// </summary>
        public void Update(Settings settings) {
            AccountName = settings.AccountName;
            PoeSessionId = settings.PoeSessionId;
            _lastUpdateCheck = settings._lastUpdateCheck;
            _configVersion = settings._configVersion;
            
            _showElapsedTime = settings._showElapsedTime;
            _showCharName = settings._showCharName;
            _showCharXp = settings._showCharXp;
            _showCharLevel = settings._showCharXp;
        }

        /// <summary>
        /// Validate all config values
        /// </summary>
        public void Validate() {
            if (AccountName == null || PoeSessionId == null || _lastUpdateCheck == null || _configVersion == null ||
                _showCharXp == null | _showCharName == null || _showCharLevel == null || _showElapsedTime == null) {
                throw new ArgumentNullException();
            }

            if (!string.IsNullOrEmpty(PoeSessionId) && !SessIdRegex.IsMatch(PoeSessionId)) {
                throw new ArgumentException("Invalid session ID");
            }

            if (!string.IsNullOrEmpty(AccountName) && AccountName.Length < 3) {
                throw new ArgumentException("Invalid account name");
            }

            if (!string.IsNullOrEmpty(_lastUpdateCheck) && !DateTime.TryParseExact(_lastUpdateCheck,
                    ConfTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _)) {
                throw new ArgumentException($"Invalid last update time (expected format {ConfTimeFormat})");
            }
                        
            if (!string.IsNullOrEmpty(_configVersion) && !VersionRegex.IsMatch(_configVersion)){
                throw new ArgumentException("Invalid config version");
            }

            if (string.IsNullOrEmpty(_showElapsedTime) ||
                !_showElapsedTime.Equals("0") &&
                !_showElapsedTime.Equals("1")) {
                throw new ArgumentException("Invalid value for show elapsed time (expected 1 or 0)");
            }

            if (string.IsNullOrEmpty(_showCharName) ||
                !_showCharName.Equals("0") &&
                !_showCharName.Equals("1")) {
                throw new ArgumentException("Invalid value for show character name (expected 1 or 0)");
            }

            if (string.IsNullOrEmpty(_showCharXp) ||
                !_showCharXp.Equals("0") &&
                !_showCharXp.Equals("1")) {
                throw new ArgumentException("Invalid value for show character xp (expected 1 or 0)");
            }

            if (string.IsNullOrEmpty(_showCharLevel) ||
                !_showCharLevel.Equals("0") &&
                !_showCharLevel.Equals("1")) {
                throw new ArgumentException("Invalid value for show character level (expected 1 or 0)");
            }
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

                case "session id":
                    PoeSessionId = val;
                    break;

                case "last update check":
                    _lastUpdateCheck = val;
                    break;

                case "show elapsed time":
                    _showElapsedTime = val;
                    break;

                case "show character name":
                    _showCharName = val;
                    break;

                case "show character xp":
                    _showCharXp = val;
                    break;

                case "show character level":
                    _showCharLevel = val;
                    break;
                
                case "config version":
                    _configVersion = val;
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

                case "session id":
                    return PoeSessionId;

                case "last update check":
                    return _lastUpdateCheck;

                case "config version":
                    return _configVersion ?? Version;

                case "show elapsed time":
                    return _showElapsedTime ?? "1";

                case "show character name":
                    return _showCharName ?? "1";

                case "show character xp":
                    return _showCharXp ?? "1";

                case "show character level":
                    return _showCharLevel ?? "1";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the last update check time to now
        /// </summary>
        public void UpdateLastUpdateTime() {
            _lastUpdateCheck = DateTime.UtcNow.ToString(ConfTimeFormat);
        }
    }
}