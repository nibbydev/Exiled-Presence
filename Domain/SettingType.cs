using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Domain {
    public enum SettingType {
        AccountName,
        PoeSessionId,
        ConfigVersion,
        LastUpdateCheck,
        ShowElapsedTime,
        ShowCharName,
        ShowCharXp,
        ShowCharLevel,
        DiscordPipe,
        PersistentTimer
    }

    public static class SettingMethods {
        private static readonly Regex SessIdRegex = new Regex("^[0-9a-fA-F]{32}$");
        private static readonly Regex VersionRegex = new Regex(@"^v\d+(\.\d+)*$");
        private const string ConfTimeFormat = "yyyy-MM-dd HH:mm";

        /// <summary>
        /// Maps enum setting types to their respective string representations
        /// </summary>
        private static readonly Dictionary<SettingType, string> KeyMap = new Dictionary<SettingType, string> {
            {SettingType.AccountName, "account name"},
            {SettingType.PoeSessionId, "session id"},
            {SettingType.ConfigVersion, "config version"},
            {SettingType.LastUpdateCheck, "last update check"},
            {SettingType.ShowElapsedTime, "show elapsed time"},
            {SettingType.ShowCharName, "show character name"},
            {SettingType.ShowCharXp, "show character xp"},
            {SettingType.ShowCharLevel, "show character level"},
            {SettingType.DiscordPipe, "discord pipe"},
            {SettingType.PersistentTimer, "persistent timer"}
        };

        /// <summary>
        /// Maps enum setting types to their respective return types
        /// </summary>
        private static readonly Dictionary<SettingType, Type> TypeMap = new Dictionary<SettingType, Type> {
            {SettingType.AccountName, typeof(string)},
            {SettingType.PoeSessionId, typeof(string)},
            {SettingType.ConfigVersion, typeof(string)},
            {SettingType.LastUpdateCheck, typeof(DateTime)},
            {SettingType.ShowElapsedTime, typeof(bool)},
            {SettingType.ShowCharName, typeof(bool)},
            {SettingType.ShowCharXp, typeof(bool)},
            {SettingType.ShowCharLevel, typeof(bool)},
            {SettingType.DiscordPipe, typeof(int)},
            {SettingType.PersistentTimer, typeof(bool)}
        };

        /// <summary>
        /// Get enum representation of string setting type 
        /// </summary>
        public static SettingType GetType(string s) {
            return KeyMap.First(t => t.Value.Equals(s)).Key;
        }

        /// <summary>
        /// Get string representation of enum setting type
        /// </summary>
        public static string GetKey(SettingType s) {
            return KeyMap[s];
        }

        /// <summary>
        /// Get return type of setting
        /// </summary>
        public static Type GetValueType(SettingType s) {
            return TypeMap[s];
        }

        /// <summary>
        /// Compares the setting to preset filters
        /// </summary>
        public static void Validate(this SettingType type, string val) {
            if (type.IsMandatoryVal() && string.IsNullOrEmpty(val)) {
                throw new ArgumentException($"[CONFIG] Parameter '{GetKey(type)}' requires a valid value");
            }

            bool hasError;
            string suggestion = null;

            switch (type) {
                case SettingType.AccountName:
                    hasError = !string.IsNullOrEmpty(val) && val.Length < 3;
                    break;

                case SettingType.PoeSessionId:
                    hasError = !string.IsNullOrEmpty(val) && !SessIdRegex.IsMatch(val);
                    break;

                case SettingType.ConfigVersion:
                    hasError = string.IsNullOrEmpty(val) || !VersionRegex.IsMatch(val);
                    break;

                case SettingType.LastUpdateCheck:
                    hasError = !string.IsNullOrEmpty(val) && !DateTime.TryParseExact(val, ConfTimeFormat,
                                   CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _);
                    suggestion = "expected format " + ConfTimeFormat;
                    break;

                case SettingType.ShowElapsedTime:
                case SettingType.ShowCharLevel:
                case SettingType.ShowCharXp:
                case SettingType.ShowCharName:
                case SettingType.PersistentTimer:
                    hasError = !string.Equals(val, "0") && !string.Equals(val, "1");
                    suggestion = "expected 1 or 0";
                    break;

                case SettingType.DiscordPipe:
                    hasError = !int.TryParse(val, out var pipeNr) || pipeNr < -1 || pipeNr > 8;
                    suggestion = "expected -1 or 0 to 8";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (hasError) {
                var msg = $"[CONFIG] Invalid value for '{GetKey(type)}'";
                if (!string.IsNullOrEmpty(suggestion)) msg += $" ({suggestion})";

                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Does the setting have to explicitly have a valid value declared in the config?
        /// </summary>
        public static bool IsMandatoryVal(this SettingType type) {
            switch (type) {
                case SettingType.ConfigVersion:
                case SettingType.LastUpdateCheck:
                case SettingType.ShowElapsedTime:
                case SettingType.ShowCharName:
                case SettingType.ShowCharXp:
                case SettingType.ShowCharLevel:
                case SettingType.PersistentTimer:
                    return true;
                case SettingType.AccountName:
                case SettingType.PoeSessionId:
                case SettingType.DiscordPipe:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Does the setting have to be present in the config?
        /// </summary>
        public static bool IsMandatory(this SettingType type) {
            switch (type) {
                case SettingType.AccountName:
                case SettingType.PoeSessionId:
                case SettingType.ConfigVersion:
                case SettingType.LastUpdateCheck:
                case SettingType.ShowElapsedTime:
                case SettingType.ShowCharName:
                case SettingType.ShowCharXp:
                case SettingType.ShowCharLevel:
                case SettingType.PersistentTimer:
                    return true;
                case SettingType.DiscordPipe:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}