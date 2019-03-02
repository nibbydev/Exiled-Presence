using System;
using System.Linq;
using System.Threading;
using Domain;

namespace Service {
    public static class Service {
        private const string WindowTitle = "Path of Exile";
        public static string SessId { private get; set; }
        public static string AccountName { private get; set; }
        
        private static LogParser _parser;
        private static ProcMon _procMon;
        private static RpClient _rpClient;

        /// <summary>
        /// Entry point
        /// </summary>
        public static void Init() {
            Console.WriteLine("Starting rich presence service");
            
            if (string.IsNullOrEmpty(SessId) || string.IsNullOrEmpty(AccountName)) {
                Console.WriteLine("No sessid or accoutname set");
            }
            
            // Register actions
            LogRegExps.RegExpList.First(t => t.Type == LogType.AreaChange).ParseAction = ActionAreaChange;
            LogRegExps.RegExpList.First(t => t.Type == LogType.StatusChange).ParseAction = ActionStatusChange;
            LogRegExps.RegExpList.First(t => t.Type == LogType.CharacterSelect).ParseAction = ActionCharacterSelect;
            LogRegExps.RegExpList.First(t => t.Type == LogType.LoginScreen).ParseAction = ActionLoginScreen;
            Web.SessId = SessId;

            // Create a process monitor (and run it as a task) that reacts to the game client being launched and closed
            _procMon = new ProcMon(WindowTitle) {
                // If the game was already running, then start action will be called
                ActionProcessStart = ActionProcessStart,
                // If the game was not running then the stop action will not be called
                ActionProcessStop = ActionProcessStop
            }.RunAsTask();

            // Currently the GUI/CLI is not implemented so our main loop is just a continuous sleep statement
            try {
                while (true) {
                    Thread.Sleep(1000);
                }
            } finally {
                _parser?.Stop();
                _procMon?.Stop();
                _rpClient?.Stop();
            }
        }

        #region Process Actions

        /// <summary>
        /// Called when game client is launched
        /// </summary>
        private static void ActionProcessStart() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[EVENT] Game start");
            Console.ResetColor();
            
            // Get the expected log path or null by traversing using the game executable location
            var logPath = Utility.GetLogFilePath(WindowTitle);

            // Make sure the the last instances were disposed of
            if (_parser != null) throw new Exception("Last log parser was not disposed of!");
            if (_rpClient != null) throw new Exception("Last rich presence client was not disposed of!");
            
            // Create a rich presence client and run it as a task
            _rpClient = new RpClient(AccountName).RunAsTask(); 

            // Create a new parser and run it as a task
            _parser = new LogParser(logPath) {
                UpdateCharacter = _rpClient.UpdateCharacter,
                ActionCharacterSelect = ActionCharacterSelect,
                ActionLoginScreen = ActionLoginScreen,
                ActionAreaChange = ActionAreaChange,
                ActionStatusChange = ActionStatusChange
            }.RunAsTask();
        }

        /// <summary>
        /// Called when game client is closed
        /// </summary>
        private static void ActionProcessStop() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[EVENT] Game stop");
            Console.ResetColor();

            // Disconnect the RP client
            _rpClient?.Stop();
            _rpClient = null;

            // Stop parsing the log
            _parser?.Stop();
            _parser = null;
        }
        
        #endregion
        
        #region Log Actions

        /// <summary>
        /// User is on login screen
        /// </summary>
        private static void ActionLoginScreen(LogMatch logMatch) {
            _rpClient?.UpdateLoginScreen();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[EVENT] Player is on login screen");
            Console.ResetColor();
        }

        /// <summary>
        /// User is in character select
        /// </summary>
        private static void ActionCharacterSelect(LogMatch logMatch) {
            _rpClient?.UpdateCharacterSelect();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[EVENT] Player is in character select");
            Console.ResetColor();
        }

        /// <summary>
        /// User is in the game and changed areas
        /// </summary>
        private static void ActionAreaChange(LogMatch logMatch) {
            if (logMatch == null || logMatch.Type != LogType.AreaChange) {
                throw new ArgumentException("Invalid match passed");
            }

            var areaName = logMatch.Match.Groups[2].Value;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[EVENT] Player switched areas to {areaName}");
            Console.ResetColor();
            
            _rpClient?.UpdateArea(areaName);
        }

        /// <summary>
        /// User is in the game and turned DND/AFK ON/OFF
        /// </summary>
        private static void ActionStatusChange(LogMatch logMatch) {
            if (logMatch == null || logMatch.Type != LogType.StatusChange) {
                throw new ArgumentException("Invalid match passed");
            }

            var mode = logMatch.Match.Groups[2].Value; // DND or AFK
            var on = logMatch.Match.Groups[3].Value.Equals("ON"); // ON or OFF
            var msg = on ? logMatch.Match.Groups[4].Value : null; // Status message or null

            _rpClient?.UpdateStatus(mode, on, msg);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[EVENT] Player switched {mode} {on} with message {msg}");
            Console.ResetColor();
        }

        #endregion
    }
}