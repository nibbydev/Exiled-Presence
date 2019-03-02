using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service {
    /// <summary>
    /// A process monitor that executes actions when the process is started/stopped. Process is found based on window
    /// title.
    /// </summary>
    public class ProcMon {
        private const int PollRateActive = 5000;
        private const int PollRateInactive = 5000;
        private readonly string _windowTitle;
        public bool IsProcRunning { get; private set; }
        private bool _lastIsProcRunning;
        private bool _run = true;
        public Action ActionProcessStart { get; set; }
        public Action ActionProcessStop { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcMon(string windowTitle) {
            _windowTitle = windowTitle;
        }

        /// <summary>
        /// Stops the main loop
        /// </summary>
        public void Stop() {
            _run = false;
        }

        /// <summary>
        /// Runs the main loop as a Task
        /// </summary>
        public ProcMon RunAsTask() {
            new Task(Run).Start();
            return this;
        }

        /// <summary>
        /// Main loop of the class
        /// </summary>
        public void Run() {
            do {
                // todo: replace with system events
                IsProcRunning = Win32.IsRunning(_windowTitle);

                // If the state of the process has changed from either OFF->ON or vice-versa
                if (_lastIsProcRunning != IsProcRunning) {
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

                // todo: notify
                // Sleep for x MS depending on the process state
                Thread.Sleep(IsProcRunning ? PollRateActive : PollRateInactive);
            } while (_run);
        }
    }
}