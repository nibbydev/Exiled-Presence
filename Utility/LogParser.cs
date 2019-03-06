using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Service;

namespace Utility {
    /// <summary>
    /// Class for parsing the game's log file and calling actions based on matches
    /// </summary>
    public class LogParser {
        private const long LogOffsetBytes = 1024 * 1024 * 5;
        private const int ReadDelayMs = 1000;
        private readonly string _path;
        private bool _run = true;
        private bool _eof;

        private LogMatch _lastAreaMatch;
        private LogMatch _lastMatch;

        public Action UpdateCharacter { private get; set; }
        public Action<LogMatch> ActionCharacterSelect { private get; set; }
        public Action<LogMatch> ActionLoginScreen { private get; set; }
        public Action<LogMatch> ActionAreaChange { private get; set; }
        public Action<LogMatch> ActionStatusChange { private get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LogParser(string path) {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("Log file path cannot be empty");
            }

            if (!File.Exists(path)) {
                throw new FileNotFoundException("Log does not exist at expected location: " + path);
            }

            _path = path;
        }

        /// <summary>
        /// Stops the main loop
        /// </summary>
        public void Stop() {
            _run = false;
            // todo: wh.Close();
        }

        /// <summary>
        /// Runs the main loop as a Task
        /// </summary>
        public LogParser RunAsTask() {
            new Task(Run).Start();
            return this;
        }

        /// <summary>
        /// Main loop of the class
        /// </summary>
        public void Run() {
            // Create an event we can signal when a new log line has been added
            var wh = new AutoResetEvent(false);
            // Create a file watcher
            var fsw = new FileSystemWatcher(".") {
                Filter = _path,
                EnableRaisingEvents = true
            };

            // Subscribe the event to the file watcher
            fsw.Changed += (s, e) => wh.Set();

            // Both need to be disposed
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8)) {
                Console.WriteLine(@"Parsing log file...");
                
                // If the file is more than x bytes, set the read position before the last x bytes
                if (fs.Length > LogOffsetBytes) {
                    fs.Seek(fs.Length - LogOffsetBytes, SeekOrigin.Begin);
                    
                    var skippedPercentage = Math.Round((double) fs.Position / fs.Length * 100f);
                    var skippedMb = Math.Round((fs.Length - LogOffsetBytes) / 1024f / 1024f);
                    
                    Console.WriteLine($@"Skipped {skippedMb}MB ({skippedPercentage}%) of the log");
                }

                // Main loop
                while (_run) {
                    var s = sr.ReadLine();

                    if (s == null) {
                        // This is ran the first time EOF is reached
                        if (!_eof) {
                            _eof = true;
                            Console.WriteLine(@"Finished parsing log file");
                            CheckEof();
                        }

                        // Wait for next signal
                        wh.WaitOne(ReadDelayMs);
                    } else {
                        // Log line was a valid string
                        MatchLogLine(s);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to find the last occurred event when initially going through the log file. This will no longer run 
        /// after EOF has been reached at least once.
        /// </summary>
        private void CheckEof() {
            // There was no match in the entire log file. Initial presence will be the default one
            if (_lastMatch == null) {
                return;
            }

            // todo: remove
            Console.WriteLine($@"Found last event from log: {_lastMatch.Type}");

            if (_lastAreaMatch != null) {
                Console.WriteLine($@"Found last area event from log: {_lastAreaMatch.Match.Groups[2].Value}");
            }

            // Set initial presence
            switch (_lastMatch.Type) {
                case LogType.AreaChange:
                    //UpdateCharacter?.Invoke(); // AreaChange already calls UpdateCharacter
                    ActionAreaChange?.Invoke(_lastMatch);
                    return;

                case LogType.StatusChange:
                    if (_lastAreaMatch != null) {
                        ActionAreaChange?.Invoke(_lastAreaMatch);
                    } else {
                        UpdateCharacter?.Invoke();
                    }

                    ActionStatusChange?.Invoke(_lastMatch);
                    return;

                case LogType.CharacterSelect:
                    ActionCharacterSelect?.Invoke(null);
                    return;

                case LogType.LoginScreen:
                    ActionLoginScreen?.Invoke(null);
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Attempts to match the log line against all predefined regex patterns. If there was a match, the action
        /// associated with the pattern will be called.
        /// </summary>
        private void MatchLogLine(string s) {
            // Match log line against all regular expressions
            foreach (var logRegExp in LogRegExps.RegExpList) {
                foreach (var regExp in logRegExp.RegExps) {
                    var match = regExp.Match(s);
                    if (!match.Success) continue;

                    var logMatch = new LogMatch {
                        Type = logRegExp.Type,
                        Match = match,
                        Msg = s
                    };

                    _lastMatch = logMatch;
                    if (logRegExp.Type == LogType.AreaChange) {
                        _lastAreaMatch = logMatch;
                    }

                    // Program is still parsing log after launch. EOF is not yet reached. These log entries might be
                    // from weeks ago.
                    if (!_eof) return;

                    // Now that we're at the EOF. If there was a match, invoke its action
                    logRegExp.ParseAction?.Invoke(logMatch);
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the game log file's full path based on process path
        /// </summary>
        public static string GetLogFilePath(string exePath) {
            if (string.IsNullOrEmpty(exePath)) {
                return null;
            }

            var gameDir = Path.GetDirectoryName(exePath);
            if (gameDir == null) {
                return null;
            }

            var logFile = Path.Combine(gameDir, "logs", "Client.txt");
            return File.Exists(logFile) ? logFile : null;
        }
    }
}