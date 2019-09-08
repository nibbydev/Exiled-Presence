using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace Service {
    public class Controller : IDisposable {
        private readonly LogParser _logParser;
        private readonly ProcMon _procMon;
        private readonly RpcClient _rpcClient;
        private readonly Settings _settings;

        private LogMatch _lastAreaMatch;
        private LogMatch _lastMatch;

        /// <summary>
        /// Constructor
        /// </summary>
        public Controller(Settings settings) {
            _settings = settings;

            // Create a process monitor that reacts to the game client being launched and closed
            _procMon = new ProcMon(Settings.GameWindowTitle) {
                ActionProcessStart = ActionProcessStart,
                ActionProcessStop = ActionProcessStop
            };

            // Create a parser for the game's log
            _logParser = new LogParser {
                LogAction = LogAction,
                EofAction = SetInitialPresence
            };

            // Create an RPC client
            _rpcClient = new RpcClient(_settings);
        }

        /// <summary>
        /// Service initializer
        /// </summary>
        public void Initialize() {
            Console.WriteLine(@"Starting controller");

            _procMon.Initialize();
        }

        /// <summary>
        /// Disposes of used resources
        /// </summary>
        public void Dispose() {
            _logParser.Dispose();
            _procMon.Dispose();
            _rpcClient.Dispose();
        }

        #region Process monitor callbacks

        /// <summary>
        /// Called when game client is launched
        /// </summary>
        private void ActionProcessStart() {
            Console.WriteLine(@"[EVENT] Game start");

            // Get the expected log path or null by using the game executable location
            var exePath = Win32.FindProcessPath(Settings.GameWindowTitle);
            var logPath = Misc.GetPoeLogPath(exePath);

            // Run log parser
            _rpcClient.Initialize();
            _logParser.Initialize(logPath);
        }

        /// <summary>
        /// Called when game client is closed
        /// </summary>
        private void ActionProcessStop() {
            Console.WriteLine(@"[EVENT] Game stop");
            _logParser.Dispose();
            _rpcClient.Dispose();
        }

        #endregion

        #region Log parser callbacks

        /// <summary>
        /// Attempts to match the log line against all predefined regex patterns. If there was a match, the action
        /// associated with the pattern will be called.
        /// </summary>
        private void LogAction(string line) {
            // Match log line against all regular expressions
            foreach (var logRegExp in LogRegExps.RegExpList) {
                foreach (var regExp in logRegExp.RegExps) {
                    var match = regExp.Match(line);
                    if (!match.Success) continue;

                    var logMatch = new LogMatch {
                        Type = logRegExp.Type,
                        Match = match,
                        Msg = line
                    };

                    _lastMatch = logMatch;
                    if (logRegExp.Type == LogType.AreaChange) {
                        _lastAreaMatch = logMatch;
                    }

                    // EOF is not yet reached
                    if (!_logParser.IsEof) return;

                    // If there was a match, invoke an action
                    switch (logRegExp.Type) {
                        case LogType.AreaChange:
                            ActionAreaChange(logMatch);
                            break;
                        case LogType.StatusChange:
                            ActionStatusChange(logMatch);
                            break;
                        case LogType.CharacterSelect:
                            ActionCharacterSelect(logMatch);
                            break;
                        case LogType.LoginScreen:
                            ActionLoginScreen(logMatch);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Sets initial presence according to last event found in the log file
        /// </summary>
        private void SetInitialPresence() {
            // There was no match in the entire log file. Initial presence will be the default one
            if (_lastMatch == null) {
                return;
            }

            Console.WriteLine($@"Found last event from log: {_lastMatch.Type}");

            if (_lastAreaMatch != null) {
                Console.WriteLine($@"Found last area event from log: {_lastAreaMatch.Match.Groups[2].Value}");
            }

            // Set initial presence
            switch (_lastMatch.Type) {
                case LogType.AreaChange:
                    ActionAreaChange(_lastMatch);
                    return;

                case LogType.StatusChange:
                    if (_lastAreaMatch != null) ActionAreaChange(_lastAreaMatch);
                    ActionStatusChange(_lastMatch);
                    return;

                case LogType.CharacterSelect:
                    ActionCharacterSelect(null);
                    return;

                case LogType.LoginScreen:
                    ActionLoginScreen(null);
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Log actions

        /// <summary>
        /// User is on login screen
        /// </summary>
        private void ActionLoginScreen(LogMatch logMatch) {
            _rpcClient.PresenceUpdateLoginScreen();
            Console.WriteLine(@"[EVENT] Player is on login screen");
        }

        /// <summary>
        /// User is in character select
        /// </summary>
        private void ActionCharacterSelect(LogMatch logMatch) {
            _rpcClient.PresenceUpdateCharacterSelect();
            Console.WriteLine(@"[EVENT] Player is in character select");
        }

        /// <summary>
        /// User is in the game and changed areas
        /// </summary>
        private void ActionAreaChange(LogMatch logMatch) {
            if (logMatch == null || logMatch.Type != LogType.AreaChange) {
                throw new ArgumentException("Invalid match passed");
            }

            var areaName = logMatch.Match.Groups[2].Value;
            _rpcClient.PresenceUpdateArea(areaName);
            Console.WriteLine($@"[EVENT] Player switched areas to {areaName}");
        }

        /// <summary>
        /// User is in the game and turned DND/AFK ON/OFF
        /// </summary>
        private void ActionStatusChange(LogMatch logMatch) {
            if (logMatch == null || logMatch.Type != LogType.StatusChange) {
                throw new ArgumentException("Invalid match passed");
            }

            var mode = logMatch.Match.Groups[2].Value; // DND or AFK
            var on = logMatch.Match.Groups[3].Value.Equals("ON"); // ON or OFF
            var msg = on ? logMatch.Match.Groups[5].Value : null; // Status message or null

            _rpcClient.PresenceUpdateStatus(mode, on, msg);
            Console.WriteLine($@"[EVENT] Player switched {mode} {on} with message '{msg}'");
        }

        #endregion
    }
}