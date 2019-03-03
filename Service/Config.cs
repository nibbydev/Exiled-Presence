using System;
using System.IO;
using Domain;

namespace Service {
    public static class Config {
        private const Environment.SpecialFolder AppDataFolder = Environment.SpecialFolder.LocalApplicationData;
        private const string FolderName = "Exiled Presence";
        private const string ConfigName = "config.json";
        
        private static readonly string AppDataPath = Environment.GetFolderPath(AppDataFolder);
        private static readonly string CfgFolderPath = Path.Combine(AppDataPath, FolderName);
        private static readonly string CfgFilePath = Path.Combine(CfgFolderPath, ConfigName);
        public static Settings Settings { get; } = new Settings();

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
        public static void LoadConfig() {
            if (!Directory.Exists(CfgFolderPath)) {
                Directory.CreateDirectory(CfgFolderPath);
            }

            if (File.Exists(CfgFilePath)) {
                ReadConfig();
            } else {
                SaveConfig();
            }
        }

        /// <summary>
        /// Read config and overwrite values
        /// </summary>
        private static void ReadConfig() {
            using (var streamReader = File.OpenText(CfgFilePath)) {
                var configString = streamReader.ReadToEnd();
                var settings = Utility.Deserialize<Settings>(configString);
                Settings.Update(settings);
            }
        }

        /// <summary>
        /// Overwrite config
        /// </summary>
        public static void SaveConfig() {
            using (var streamWriter = new StreamWriter(CfgFilePath)) {
                var rawData = Utility.Serialize(Settings);
                streamWriter.Write(rawData);
            }
        }
    }
}