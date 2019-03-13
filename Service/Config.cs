using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Domain;
using Service.Properties;

namespace Service {
    public class Config {
        private const string ConfigFileName = "config.txt";

        private static readonly string AppDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly string CfgFolderPath = Path.Combine(AppDataPath, Settings.ProgramName);
        private static readonly string CfgFilePath = Path.Combine(CfgFolderPath, ConfigFileName);
        private static readonly Regex CfgRegex = new Regex(@"^(\s*)(.*?)(\s*=\s*)(.*?)(\s*)$");

        private readonly Settings _settings;

        /// <summary>
        /// Constructor
        /// </summary>
        public Config(Settings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Loads config on program start
        /// </summary>
        public void Load() {
            if (!Directory.Exists(CfgFolderPath)) {
                Directory.CreateDirectory(CfgFolderPath);
            }

            if (File.Exists(CfgFilePath)) {
                try {
                    Read();
                } catch {
                    Save();
                    throw;
                }
            }

            try {
                _settings.Validate();
            } catch {
                _settings.Reset();
                throw;
            }

            Save();
        }

        /// <summary>
        /// Read config and overwrite values
        /// </summary>
        private void Read() {
            string conf;
            using (var sr = File.OpenText(CfgFilePath)) {
                conf = sr.ReadToEnd();
            }

            if (string.IsNullOrEmpty(conf.Trim())) {
                throw new Exception("Config was empty");
            }

            Parse(conf);
        }

        /// <summary>
        /// Attempt to read values from config string and load them into settings
        /// </summary>
        private void Parse(string conf) {
            var splitConf = conf.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            foreach (var s in splitConf) {
                if (string.IsNullOrEmpty(s) || s.Trim().StartsWith("#")) continue;

                var match = CfgRegex.Match(s);
                if (!match.Success) continue;

                var key = match.Groups[2].Value.Trim();
                var val = match.Groups[4].Value.Trim();

                if (val.StartsWith("#")) continue;

                _settings.ParseValue(key, val);
            }
        }

        /// <summary>
        /// Overwrite/create config
        /// </summary>
        public void Save() {
            // Get the base config and split it line by line
            var baseConf = Encoding.Default.GetString(Resources.BaseConfig)
                .Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            using (var sr = new StreamWriter(CfgFilePath)) {
                // Loop though each line, replacing values with current settings
                foreach (var s in baseConf) {
                    var match = CfgRegex.Match(s);

                    if (match.Success) {
                        var val = _settings.GetValue(match.Groups[2].Value);
                        var replacement = CfgRegex.Replace(s, "$1$2$3") + (val ?? "#none");

                        sr.WriteLine(replacement);
                    } else {
                        sr.WriteLine(s);
                    }
                }
            }
        }

        /// <summary>
        /// Opens the config file in the default text editor
        /// </summary>
        public void OpenInEditor() {
            if (!Directory.Exists(CfgFolderPath)) {
                Directory.CreateDirectory(CfgFolderPath);
            }

            if (!File.Exists(CfgFilePath)) {
                Save();
            }

            Misc.OpenPath(CfgFilePath);
        }
    }
}