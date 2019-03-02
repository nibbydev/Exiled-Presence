using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Domain;

namespace Service {
    public class RpClient {
        private const int CharacterUpdateDelay = 60 * 1000;
        private const int PollDelay = 1000;
        private const string ClientId = "551089446460850176";
        private static readonly ConsoleLogger Logger = new ConsoleLogger {Level = LogLevel.Warning, Coloured = true};

        private readonly string _accountName;
        private DiscordRpcClient _client;
        private Character _character;
        private Timer _charUpdateTimer;
        private DateTime? _lastAreaChange;
        private string _lastStateAreaMsg;
        private RichPresence _presence;
        private bool _run = true;
        private bool _hasUpdate;

        /// <summary>
        /// Constructor
        /// </summary>
        public RpClient(string accountName) {
            _accountName = accountName;

            _presence = new RichPresence {
                Assets = new Assets {
                    LargeImageKey = "misc_logo"
                }
            };

            _client = new DiscordRpcClient(ClientId, true, -1, Logger);

            _client.OnReady += OnReady;
            _client.OnClose += OnClose;
            _client.OnError += OnError;

            _client.OnConnectionEstablished += OnConnectionEstablished;
            _client.OnConnectionFailed += OnConnectionFailed;

            _client.SetPresence(_presence);
            _client.Initialize();
        }

        /// <summary>
        /// Stops the main loop
        /// </summary>
        public void Stop() {
            _run = false;
            _charUpdateTimer?.Dispose();
            _client?.Dispose();
        }

        /// <summary>
        /// Runs the main loop as a Task
        /// </summary>
        public RpClient RunAsTask() {
            new Task(Run).Start();
            return this;
        }

        /// <summary>
        /// Main loop of the class
        /// </summary>
        public void Run() {
            while (_run) {
                if (_hasUpdate) {
                    _hasUpdate = false;
                    _client?.SetPresence(_presence);
                }

                _client?.Invoke();
                Thread.Sleep(PollDelay);
            }
        }

        /// <summary>
        /// Requests current character from the API and asynchronously updates the presence
        /// </summary>
        public async void UpdateCharacter() {
            // More than x minutes have passed since player switched areas
            if (_lastAreaChange?.AddMinutes(5) < DateTime.UtcNow) {
                Console.WriteLine("Ignored char update request due to inactivity for {0} min",
                    _lastAreaChange == null ? 0 : (DateTime.UtcNow - _lastAreaChange).Value.Minutes);
                return;
            }

            // todo: if character is not in an area that does not grant xp
            // todo: disable character updates when xp is off?

            _character = await Web.GetLastActiveChar(_accountName);
            if (_character == null) {
                return;
            }
            
            UpdateCharacterData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[EVENT] Got character from api: {_character.Name}");
            Console.ResetColor();
        }

        /// <summary>
        /// Resets active character
        /// </summary>
        private void ResetCharacter() {
            _character = null;
            _charUpdateTimer?.Dispose();
            _lastAreaChange = null;
        }

        /// <summary>
        /// Updates the presence AFK or DND status
        /// </summary>
        public void UpdateStatus(string mode, bool on, string message) {
            _presence.State = on ? $"{_lastStateAreaMsg} ({mode})" : _lastStateAreaMsg;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence area
        /// </summary>
        public void UpdateArea(string area) {
            // First character login, get initial info
            if (_character == null) {
                UpdateCharacter();

                // Disabled currently
                // Create a timer that runs every x MS and updates character stats
                // _charUpdateTimer = new Timer(state => UpdateCharacter(), null, CharacterUpdateDelay, CharacterUpdateDelay);
            }

            _presence.Timestamps = Timestamps.Now;
            _presence.State = $"In {area}";

            _lastAreaChange = DateTime.UtcNow;
            _lastStateAreaMsg = _presence.State;
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to login screen
        /// </summary>
        public void UpdateLoginScreen() {
            ResetCharacter();

            _presence.Assets.LargeImageKey = Utility.GetArtKey();
            _presence.Assets.LargeImageText = "Nothing particularly noteworthy going on right now";
            _presence.Details = "In login screen";
            _presence.State = null;
            _presence.Timestamps = Timestamps.Now;
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to character select
        /// </summary>
        public void UpdateCharacterSelect() {
            ResetCharacter();

            _presence.Assets.LargeImageKey = Utility.GetArtKey();
            _presence.Assets.LargeImageText = "Nothing particularly noteworthy going on right now";
            _presence.Details = "In character select";
            _presence.State = null;
            _presence.Timestamps = Timestamps.Now;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence with the current character data
        /// </summary>
        private void UpdateCharacterData() {
            var largeAssetKey = Utility.GetArtKey(_character.Class);
            var xpPercent = Utility.GetPercentToNextLevel(_character.Level, _character.Experience);
            var baseClass = Utility.GetBaseClass(_character.Class);

            _presence.Assets.LargeImageKey = largeAssetKey;
            _presence.Details = $"Lvl {_character.Level} {_character.Class} ({_character.League})";
            _presence.Assets.LargeImageText = baseClass.Equals(_character.Class)
                ? baseClass
                : $"{baseClass} ascended as {_character.Class}";

            //Presence.State = $"{xpPercent}% xp to next level";
            _hasUpdate = true;
        }

        #region State Events

        private static void OnReady(object sender, ReadyMessage args) {
            //This is called when we are all ready to start receiving and sending discord events. 
            // It will give us some basic information about discord to use in the future.

            //It can be a good idea to send a inital presence update on this event too, just to setup the inital game state.
            Console.WriteLine("On Ready. RPC Version: {0}", args.Version);
        }

        private static void OnClose(object sender, CloseMessage args) {
            //This is called when our client has closed. The client can no longer send or receive events after this message.
            // Connection will automatically try to re-establish and another OnReady will be called (unless it was disposed).
            Console.WriteLine("Lost Connection with client because of '{0}'", args.Reason);
        }

        private static void OnError(object sender, ErrorMessage args) {
            //Some error has occured from one of our messages. Could be a malformed presence for example.
            // Discord will give us one of these events and its upto us to handle it
            Console.WriteLine("Error occured within discord. ({1}) {0}", args.Message, args.Code);
        }

        #endregion

        #region Pipe Connection Events

        private static void OnConnectionEstablished(object sender, ConnectionEstablishedMessage args) {
            //This is called when a pipe connection is established. The connection is not ready yet, but we have at least found a valid pipe.
            Console.WriteLine("Pipe Connection Established. Valid on pipe #{0}", args.ConnectedPipe);
        }

        private static void OnConnectionFailed(object sender, ConnectionFailedMessage args) {
            //This is called when the client fails to establish a connection to discord. 
            // It can be assumed that Discord is unavailable on the supplied pipe.
            Console.WriteLine("Pipe Connection Failed. Could not connect to pipe #{0}", args.FailedPipe);
        }

        #endregion
    }
}