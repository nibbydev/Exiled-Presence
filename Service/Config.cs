using System;
using System.IO;
using System.Text.RegularExpressions;
using Utility;

namespace Service {
    public static class Config {
        public static readonly Regex SessIdRegex = new Regex("^[0-9a-fA-F]{32}$");

        private const Environment.SpecialFolder AppDataFolder = Environment.SpecialFolder.LocalApplicationData;

        private static readonly string AppDataPath = Environment.GetFolderPath(AppDataFolder);
        private static readonly string CfgFolderPath = Path.Combine(AppDataPath, Settings.ProgramName);
        private static readonly string CfgFilePath = Path.Combine(CfgFolderPath, Settings.ConfigFileName);
        public static Settings Settings { get; } = new Settings();
        public static Action<string> NotifyAction { private get; set; }

        /// <summary>
        /// Removes config file from disk and resets settings
        /// </summary>
        public static void ResetConfig() {
            if (!Directory.Exists(CfgFolderPath)) {
                return;
            }

            if (!File.Exists(CfgFilePath)) {
                return;
            }

            File.Delete(CfgFilePath);
            Settings.Update(new Settings());
        }

        /// <summary>
        /// Loads config into static context on program start
        /// </summary>
        public static bool LoadConfig() {
            if (!Directory.Exists(CfgFolderPath)) {
                Directory.CreateDirectory(CfgFolderPath);
            }

            if (File.Exists(CfgFilePath)) {
                try {
                    ReadConfig();
                } catch {
                    NotifyAction?.Invoke("Invalid config syntax");
                    SaveConfig();
                    return false;
                }
            }

            if (!Settings.Validate(out var errorMsg)) {
                NotifyAction?.Invoke($"Invalid config ({errorMsg})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read config and overwrite values
        /// </summary>
        private static void ReadConfig() {
            using (var streamReader = File.OpenText(CfgFilePath)) {
                var configString = streamReader.ReadToEnd();
                
                var settings = JsonUtility.Deserialize<Settings>(configString);
                Settings.Update(settings);
            }
        }

        /// <summary>
        /// Overwrite config
        /// </summary>
        public static void SaveConfig() {
            using (var streamWriter = new StreamWriter(CfgFilePath)) {
                var rawData = JsonUtility.Serialize(Settings);
                streamWriter.Write(rawData);
            }
        }

        /// <summary>
        /// Opens the config file in the default text editor
        /// </summary>
        public static void OpenConfig() {
            if (!Directory.Exists(CfgFolderPath)) {
                Directory.CreateDirectory(CfgFolderPath);
            }

            if (!File.Exists(CfgFilePath)) {
                SaveConfig();
            }

            System.Diagnostics.Process.Start(CfgFilePath);
        }

        #region Propagators

        /// <summary>
        /// Setter and validator for account name
        /// </summary>
        public static string AccountNameInputPropagate(string input) {
            if (input.Length < 3) {
                return "ERROR. Invalid account name passed!";
            }

            Settings.AccountName = input;
            SaveConfig();

            return "OK. Account name set.";
        }

        /// <summary>
        /// Setter and validator for session id
        /// </summary>
        public static string SessIdInputPropagate(string input) {
            var regex = new Regex(@"^[0-9a-fA-F]{32}$");
            if (!regex.Match(input).Success) {
                return "ERROR. Invalid POESESSID passed!";
            }

            Settings.PoeSessionId = input;
            SaveConfig();

            return "OK. POESESSID set.";
        }

        #endregion
    }
}