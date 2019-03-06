using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Domain;
using Utility;

namespace Service {
    public class RpClient {
        private static readonly ConsoleLogger Logger = new ConsoleLogger {Level = LogLevel.Warning, Coloured = true};
        private readonly DiscordRpcClient _client;
        private readonly RichPresence _presence;
        private Character _character;
        private DateTime? _lastCharUpdate;
        private bool _run = true;
        private bool _hasUpdate;
        private Area _currentArea;

        /// <summary>
        /// Constructor
        /// </summary>
        public RpClient() {
            _presence = new RichPresence {
                Assets = new Assets {
                    LargeImageKey = "misc_logo",
                    LargeImageText = $"{Settings.ProgramName} {Settings.Version}"
                }
            };

            _client = new DiscordRpcClient(Settings.DiscordAppId, true, -1, Logger);

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
                Thread.Sleep(Settings.PresencePollDelayMs);
            }
        }

        /// <summary>
        /// Requests current character from the API and asynchronously updates the presence
        /// </summary>
        public async void UpdateCharacter() {
            // More than x has passed since last char update
            if (_lastCharUpdate?.AddSeconds(Settings.CharacterUpdateDelaySec) > DateTime.UtcNow) {
                return;
            }

            // todo: if character is not in an area that does not grant xp

            Character character;
            try {
                character = await Web.GetLastActiveChar(Config.Settings.AccountName, Config.Settings.PoeSessionId);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return;
            }

            // Something somehow went wrong, don't overwrite current character data
            if (character == null) {
                return;
            }
            
            _lastCharUpdate = DateTime.UtcNow;
            _character = character;
            UpdateCharacterData();

            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($@"[EVENT] Got character from api: {_character.Name}");
            Console.ResetColor();
        }

        /// <summary>
        /// Resets active character
        /// </summary>
        private void ResetCharacter() {
            _character = null;
        }

        /// <summary>
        /// Updates the presence AFK or DND status
        /// </summary>
        public void UpdateStatus(string mode, bool on, string message) {
            _presence.State = on ? mode : null;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence area
        /// </summary>
        public void UpdateArea(string areaName) {
            // Update character data on area change
            UpdateCharacter();
            
            AreaMatcher.Match(areaName, out _currentArea);
            
            UpdateSmallImageText();
            _presence.Assets.SmallImageKey = _currentArea.Key;
            _presence.Timestamps = Timestamps.Now;
            _presence.State = null;
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to login screen
        /// </summary>
        public void UpdateLoginScreen() {
            ResetCharacter();

            _presence.Assets.SmallImageKey = null;
            _presence.Assets.SmallImageText = null;
            _presence.Assets.LargeImageKey = General.GetArtKey();
            _presence.Assets.LargeImageText = $"{Settings.ProgramName} {Settings.Version}";
            _presence.State = "In login screen";
            _presence.Details = null;
            _presence.Timestamps = Timestamps.Now;
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to character select
        /// </summary>
        public void UpdateCharacterSelect() {
            ResetCharacter();

            _presence.Assets.SmallImageKey = null;
            _presence.Assets.SmallImageText = null;
            _presence.Assets.LargeImageKey = General.GetArtKey();
            _presence.Assets.LargeImageText = $"{Settings.ProgramName} {Settings.Version}";
            _presence.State = "In character select";
            _presence.Details = null;
            _presence.Timestamps = Timestamps.Now;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence with the current character data
        /// </summary>
        private void UpdateCharacterData() {
            var largeAssetKey = General.GetArtKey(_character.Class);
            var xpPercent = General.GetPercentToNextLevel(_character.Level, _character.Experience);

            _presence.Assets.LargeImageKey = largeAssetKey;
            _presence.Details = $"Playing as {_character.Name}";
            _presence.Assets.LargeImageText = $"Level {_character.Level} {_character.Class} - {xpPercent}% xp";

            UpdateSmallImageText();

            _hasUpdate = true;
        }

        private void UpdateSmallImageText() {
            if (_currentArea != null && _presence.Assets.SmallImageKey != null) {
                _presence.Assets.SmallImageText = _character == null
                    ? $"{_currentArea.Name}"
                    : $"{_currentArea.Name} in {_character.League}";
            }
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