using System.IO;
using Domain;

namespace Service {
    public class Config {
        private readonly Settings _settings;

        /// <summary>
        /// Constructor
        /// </summary>
        public Config(Settings settings) {
            _settings = settings;
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

            if (!_settings.Validate(out var errorMsg)) {
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
                _settings.Update(settings);
            }
        }

        /// <summary>
        /// Overwrite config
        /// </summary>
        public void SaveConfig() {
            using (var streamWriter = new StreamWriter(Settings.CfgFilePath)) {
                var rawData = JsonUtility.Serialize(_settings);
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

            Misc.OpenPath(Settings.CfgFilePath);
        }
    }
}