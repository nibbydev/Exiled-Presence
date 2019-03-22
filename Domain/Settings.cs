using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        public static readonly TimeSpan PresencePollInterval = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);
        public static readonly TimeSpan CharacterUpdateInterval = TimeSpan.FromSeconds(60);

        private readonly List<Setting> _defaultSettings = new List<Setting> {
            new Setting {
                Type = SettingType.AccountName,
                Val = null
            },
            new Setting {
                Type = SettingType.PoeSessionId,
                Val = null
            },
            new Setting {
                Type = SettingType.ShowCharName,
                Val = "1"
            },
            new Setting {
                Type = SettingType.ShowCharXp,
                Val = "1"
            },
            new Setting {
                Type = SettingType.ShowCharLevel,
                Val = "1"
            },
            new Setting {
                Type = SettingType.ShowElapsedTime,
                Val = "1"
            },
            new Setting {
                Type = SettingType.LastUpdateCheck,
                Val = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm")
            },
            new Setting {
                Type = SettingType.DiscordPipe,
                Val = "-1"
            },
            new Setting {
                Type = SettingType.ConfigVersion,
                Val = Version
            }
        };

        private readonly List<Setting> _userSettings = new List<Setting>();

        /// <summary>
        /// Returns the specified setting value as the specified return type.
        /// </summary>
        public T GetValOrDefault<T>(SettingType type) {
            var setting = _userSettings.FirstOrDefault(t => t.Type.Equals(type)) ??
                          _defaultSettings.First(t => t.Type.Equals(type));

            // User used unsupported type
            if (typeof(T) != SettingMethods.GetValueType(type) && typeof(T) != typeof(string)) {
                throw new ArgumentException($"Invalid type ({typeof(T)}) specified for {setting.Type}, " +
                                            $"must be {SettingMethods.GetValueType(type)} or {typeof(string)}");
            }

            // Is the out type string?
            if (typeof(T) == typeof(string)) {
                return (T) Convert.ChangeType(setting.Val, typeof(T));
            }

            // Is the out type bool?
            if (typeof(T) == typeof(bool)) {
                // Can we parse it as bool?
                if (!string.Equals(setting.Val, "1") && !string.Equals(setting.Val, "0")) {
                    throw new ArgumentException($"Cannot parse value '{setting.Val}' as bool");
                }

                return (T) Convert.ChangeType(string.Equals(setting.Val, "1"), typeof(T));
            }

            // Is the out type int?
            if (typeof(T) == typeof(int)) {
                // Can we parse it as int?
                if (!int.TryParse(setting.Val, out var parsedVal)) {
                    throw new ArgumentException($"Cannot parse value '{setting.Val}' as integer");
                }

                return (T) Convert.ChangeType(parsedVal, typeof(T));
            }

            // Is the out type double?
            if (typeof(T) == typeof(double)) {
                // Can we parse it as double?
                if (!double.TryParse(setting.Val, out var parsedVal)) {
                    throw new ArgumentException($"Cannot parse value '{setting.Val}' as double");
                }

                return (T) Convert.ChangeType(parsedVal, typeof(T));
            }

            // Is the out type DateTime?
            if (typeof(T) == typeof(DateTime)) {
                // Can we parse it as DateTime?
                if (!DateTime.TryParseExact(setting.Val, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var parsedVal)) {
                    throw new ArgumentException($"Cannot parse value '{setting.Val}' as DateTime");
                }

                return (T) Convert.ChangeType(parsedVal, typeof(T));
            }

            throw new ArgumentException("Unsupported value type");
        }

        public bool HasVal(SettingType type) {
            return _userSettings.Exists(t => t.Type.Equals(type));
        }

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

            var setting = _userSettings.FirstOrDefault(t => t.Type.Equals(type));
            if (setting == null) {
                setting = new Setting {Type = type, Val = val};
                _userSettings.Add(setting);
            } else setting.Val = val;

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
                if (type.IsMandatory() && !_userSettings.Any(t => t.Type.Equals(type))) {
                    throw new ArgumentException($"Missing config parameter '{SettingMethods.GetKey(type)}'");
                }
            }
        }

        /// <summary>
        /// Sets the last update check time to now
        /// </summary>
        public void UpdateLastUpdateTime() {
            var setting = _userSettings.First(t => t.Type.Equals(SettingType.LastUpdateCheck));
            setting.Val = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        }

        public bool IsCheckUpdates() {
            var date = GetValOrDefault<DateTime>(SettingType.LastUpdateCheck);
            return DateTime.Equals(date, DateTime.MinValue) || date < DateTime.UtcNow.Subtract(UpdateCheckInterval);
        }
    }
}