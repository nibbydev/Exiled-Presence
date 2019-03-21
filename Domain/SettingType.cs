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
        DiscordPipe
    }

    public static class SettingMethods {
        private static readonly Regex SessIdRegex = new Regex("^[0-9a-fA-F]{32}$");
        private static readonly Regex VersionRegex = new Regex(@"^v\d+(\.\d+)*$");
        private const string ConfTimeFormat = "yyyy-MM-dd HH:mm";

        private static readonly Dictionary<SettingType, string> KeyMap = new Dictionary<SettingType, string> {
            {SettingType.AccountName, "account name"},
            {SettingType.PoeSessionId, "session id"},
            {SettingType.ConfigVersion, "config version"},
            {SettingType.LastUpdateCheck, "last update check"},
            {SettingType.ShowElapsedTime, "show elapsed time"},
            {SettingType.ShowCharName, "show character name"},
            {SettingType.ShowCharXp, "show character xp"},
            {SettingType.ShowCharLevel, "show character level"},
            {SettingType.DiscordPipe, "discord pipe"}
        };

        private static readonly Dictionary<SettingType, Type> TypeMap = new Dictionary<SettingType, Type> {
            {SettingType.AccountName, typeof(string)},
            {SettingType.PoeSessionId, typeof(string)},
            {SettingType.ConfigVersion, typeof(string)},
            {SettingType.LastUpdateCheck, typeof(DateTime)},
            {SettingType.ShowElapsedTime, typeof(bool)},
            {SettingType.ShowCharName, typeof(bool)},
            {SettingType.ShowCharXp, typeof(bool)},
            {SettingType.ShowCharLevel, typeof(bool)},
            {SettingType.DiscordPipe, typeof(int)}
        };

        public static SettingType GetType(string s) {
            return KeyMap.First(t => t.Value.Equals(s)).Key;
        }

        public static string GetKey(SettingType s) {
            return KeyMap[s];
        }

        public static Type GetValueType(SettingType s) {
            return TypeMap[s];
        }

        public static void Validate(this SettingType type, string val) {
            switch (type) {
                case SettingType.AccountName:
                    if (!string.IsNullOrEmpty(val) && val.Length < 3) {
                        throw new ArgumentException("Invalid account name");
                    }

                    break;
                case SettingType.PoeSessionId:
                    if (!string.IsNullOrEmpty(val) && !SessIdRegex.IsMatch(val)) {
                        throw new ArgumentException("Invalid session ID");
                    }

                    break;
                case SettingType.ConfigVersion:
                    if (string.IsNullOrEmpty(val) || !VersionRegex.IsMatch(val)) {
                        throw new ArgumentException("Invalid config version");
                    }

                    break;
                case SettingType.LastUpdateCheck:
                    if (!string.IsNullOrEmpty(val) && !DateTime.TryParseExact(val,
                            ConfTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _)) {
                        throw new ArgumentException($"Invalid last update time (expected format {ConfTimeFormat})");
                    }

                    break;
                case SettingType.ShowElapsedTime:
                    if (!string.Equals(val, "0") && !string.Equals(val, "1")) {
                        throw new ArgumentException("Invalid value for show elapsed time (expected 1 or 0)");
                    }

                    break;
                case SettingType.ShowCharName:
                    if (!string.Equals(val, "0") && !string.Equals(val, "1")) {
                        throw new ArgumentException("Invalid value for show character name (expected 1 or 0)");
                    }

                    break;
                case SettingType.ShowCharXp:
                    if (!string.Equals(val, "0") && !string.Equals(val, "1")) {
                        throw new ArgumentException("Invalid value for show character xp (expected 1 or 0)");
                    }

                    break;
                case SettingType.ShowCharLevel:
                    if (!string.Equals(val, "0") && !string.Equals(val, "1")) {
                        throw new ArgumentException("Invalid value for show character level (expected 1 or 0)");
                    }

                    break;
                case SettingType.DiscordPipe:
                    if (!int.TryParse(val, out var pipeNr) || pipeNr < -1 || pipeNr > 8) {
                        throw new ArgumentException("Invalid value for discord pipe (expected -1 or 0 to 8)");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}