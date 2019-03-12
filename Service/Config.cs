using System.IO;
using Utility;

namespace Service {
    public class Config {
        public Settings Settings { get; } = new Settings();

        /// <summary>
        /// Removes config file from disk and resets settings
        /// </summary>
        public void ResetConfig() {
            if (!Directory.Exists(Settings.CfgFolderPath)) {
                return;
            }

            if (!File.Exists(Settings.CfgFilePath)) {
                return;
            }

            File.Delete(Settings.CfgFilePath);
            Settings.Update(new Settings());
        }

        /// <summary>
        /// Loads config into static context on program start
        /// </summary>
        public bool LoadConfig(out string msg) {
            if (!Directory.Exists(Settings.CfgFolderPath)) {
                Directory.CreateDirectory(Settings.CfgFolderPath);
            }

            if (File.Exists(Settings.CfgFilePath)) {
                try {
                    ReadConfig();
                } catch {
                    msg = "Invalid config syntax";
                    SaveConfig();
                    return false;
                }
            }

            if (!Settings.Validate(out var errorMsg)) {
                msg = $"Invalid config ({errorMsg})";
                return false;
            }

            msg = "Config loaded successfully";
            return true;
        }

        /// <summary>
        /// Read config and overwrite values
        /// </summary>
        private void ReadConfig() {
            using (var streamReader = File.OpenText(Settings.CfgFilePath)) {
                var configString = streamReader.ReadToEnd();
                
                var settings = JsonUtility.Deserialize<Settings>(configString);
                Settings.Update(settings);
            }
        }

        /// <summary>
        /// Overwrite config
        /// </summary>
        public void SaveConfig() {
            using (var streamWriter = new StreamWriter(Settings.CfgFilePath)) {
                var rawData = JsonUtility.Serialize(Settings);
                streamWriter.Write(rawData);
            }
        }

        /// <summary>
        /// Opens the config file in the default text editor
        /// </summary>
        public void OpenConfig() {
            if (!Directory.Exists(Settings.CfgFolderPath)) {
                Directory.CreateDirectory(Settings.CfgFolderPath);
            }

            if (!File.Exists(Settings.CfgFilePath)) {
                SaveConfig();
            }

            General.OpenPath(Settings.CfgFilePath);
        }
    }
}