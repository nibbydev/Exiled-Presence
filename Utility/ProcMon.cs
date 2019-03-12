using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility {
    /// <summary>
    /// A process monitor that executes actions when the process is started/stopped. Process is found based on window
    /// title.
    /// </summary>
    public class ProcMon : IDisposable {
        private readonly TimeSpan _callbackTimespan = TimeSpan.FromSeconds(5);
        private readonly string _windowTitle;
        public bool IsProcRunning { get; private set; }
        private bool _lastIsProcRunning;
        public Action ActionProcessStart { private get; set; }
        public Action ActionProcessStop { private get; set; }
        private Timer _callbackTimer;
        public bool IsInitialized => _callbackTimer != null;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ProcMon(string windowTitle) {
            _windowTitle = windowTitle;
        }

        /// <summary>
        /// Disposer. Has same functionality as close.
        /// </summary>
        public void Dispose() {
            _callbackTimer?.Dispose();
            _callbackTimer = null;
            IsProcRunning = false;
            _lastIsProcRunning = false;
        }

        /// <summary>
        /// Sets up the timer
        /// </summary>
        public void Initialize() {
            if (IsInitialized) {
                throw new Exception("Already running! Dispose first");
            }

            _callbackTimer = new Timer(Tick, null, TimeSpan.Zero, _callbackTimespan);
        }

        /// <summary>
        /// Main loop of the class
        /// </summary>
        private void Tick(object state) {
            // todo: replace with system events
            IsProcRunning = Win32.IsRunning(_windowTitle);

            // If the on state of the process has not toggled
            if (_lastIsProcRunning == IsProcRunning) return;
            
            if (IsProcRunning) {
                // Process was started
                ActionProcessStart?.Invoke();
            } else {
                // Process was stopped
                ActionProcessStop?.Invoke();
            }

            // Set last state to current state
            _lastIsProcRunning = IsProcRunning;
        }
    }
}