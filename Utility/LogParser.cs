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
    public class LogParser : IDisposable {
        private readonly TimeSpan _callbackTimespan = TimeSpan.FromMilliseconds(500);
        private const long LogOffsetBytes = 1024 * 1024 * 5;
        public Action<string> LogAction { private get; set; }
        public Action EofAction { private get; set; }
        public bool IsEof { get; private set; }

        private Stream _fs;
        private StreamReader _sr;
        private Timer _callbackTimer;

        /// <summary>
        /// Set up file tailing
        /// </summary>
        public void Initialize(string path) {
            CheckErrors(path);

            // Create streams
            _fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _sr = new StreamReader(_fs, Encoding.UTF8);

            InitialParse();

            _callbackTimer = new Timer(ReadToEof, null, TimeSpan.Zero, _callbackTimespan);
        }

        /// <summary>
        /// Check for various parameter errors
        /// </summary>
        private void CheckErrors(string path) {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("Log file path cannot be empty");
            }

            if (!File.Exists(path)) {
                throw new FileNotFoundException("Log does not exist at expected location: " + path);
            }

            if (LogAction == null) {
                throw new NullReferenceException("Log action cannot be null");
            }

            if (EofAction == null) {
                throw new NullReferenceException("EOF action cannot be null");
            }

            if (_callbackTimer != null || _fs != null || _sr != null) {
                throw new Exception("Already running! Dispose first");
            }
        }

        /// <summary>
        /// Initially parse log file
        /// </summary>
        private void InitialParse() {
            // If the file is more than x bytes, set the read position before the last x bytes
            if (_fs.Length > LogOffsetBytes) {
                _fs.Seek(_fs.Length - LogOffsetBytes, SeekOrigin.Begin);

                var skippedPercentage = Math.Round((double) _fs.Position / _fs.Length * 100f);
                var skippedMb = Math.Round((_fs.Length - LogOffsetBytes) / 1024f / 1024f);

                Console.WriteLine($@"Skipped {skippedMb}MB ({skippedPercentage}%) of the log");
            }

            // Parse the log from the offset
            ReadToEof();

            // EOF is reached
            IsEof = true;
            EofAction.Invoke();
        }

        /// <summary>
        /// Read the log file until EOF
        /// </summary>
        private void ReadToEof(object state = null) {
            string s;
            while ((s = _sr.ReadLine()) != null) {
                Console.WriteLine(s);
                LogAction.Invoke(s);
            }
        }

        /// <summary>
        /// Disposer. Has same functionality as close.
        /// </summary>
        public void Dispose() {
            _callbackTimer?.Dispose();
            _callbackTimer = null;

            _fs?.Dispose();
            _fs = null;

            _sr?.Dispose();
            _sr = null;

            IsEof = false;
        }
    }
}