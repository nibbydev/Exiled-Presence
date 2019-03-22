using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Domain;

namespace Service {
    public class RpcClient : IDisposable {
        private readonly Settings _settings;

        private DiscordRpcClient _rpcClient;
        private RichPresence _presence;
        private Character _character;
        private DateTime? _lastCharUpdate;
        private bool _hasUpdate;
        private Area _currentArea;
        private Timer _callbackTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        public RpcClient(Settings settings) {
            _settings = settings ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Sets up the timer
        /// </summary>
        public void Initialize() {
            if (_callbackTimer != null || _rpcClient != null || _presence != null) {
                throw new Exception("Already running! Dispose first");
            }

            // Create the RPC client
            _rpcClient = new DiscordRpcClient(Settings.DiscordAppId,
                _settings.GetValOrDefault<int>(SettingType.DiscordPipe));
            _rpcClient.OnReady += OnReady;
            _rpcClient.OnClose += OnClose;
            _rpcClient.OnError += OnError;
            _rpcClient.OnConnectionEstablished += OnConnectionEstablished;
            _rpcClient.OnConnectionFailed += OnConnectionFailed;

            // Define an initial presence
            _presence = new RichPresence {
                Assets = new Assets {
                    LargeImageKey = "misc_logo",
                    LargeImageText = $"{Settings.ProgramName} {Settings.Version}"
                }
            };

            // Start the client
            _rpcClient.SetPresence(_presence);
            _rpcClient.Initialize();
            _callbackTimer = new Timer(TimerCallback, null, TimeSpan.Zero, Settings.PresencePollInterval);
        }

        /// <summary>
        /// Disposer
        /// </summary>
        public void Dispose() {
            _callbackTimer?.Dispose();
            _callbackTimer = null;

            _rpcClient?.Dispose();
            _rpcClient = null;

            _presence = null;
            _character = null;
            _lastCharUpdate = null;
            _hasUpdate = false;
            _currentArea = null;
        }

        /// <summary>
        /// Updates presence, if necessary
        /// </summary>
        private void TimerCallback(object state = null) {
            if (_hasUpdate) {
                _hasUpdate = false;
                _rpcClient.SetPresence(_presence);
            }

            _rpcClient.Invoke();
        }

        /// <summary>
        /// Requests current character from the API and asynchronously updates the presence
        /// </summary>
        private async void RequestCharacterUpdate() {
            // Less than x has passed since last char update
            if (_lastCharUpdate > DateTime.UtcNow.Subtract(Settings.CharacterUpdateInterval)) {
                return;
            }

            // todo: if character is in an area that does not grant xp

            Character character;
            try {
                character = await Web.GetLastActiveChar(
                    _settings.GetValOrDefault<string>(SettingType.AccountName),
                    _settings.GetValOrDefault<string>(SettingType.PoeSessionId));
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
            PresenceUpdateCharacterData();

            Console.WriteLine($@"[EVENT] Got character from api: {_character.Name}");
        }

        #region Presence update methods

        /// <summary>
        /// Updates the presence AFK or DND status
        /// </summary>
        public void PresenceUpdateStatus(string mode, bool on, string message) {
            _presence.State = on ? mode : null;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence area
        /// </summary>
        public void PresenceUpdateArea(string areaName) {
            // Update character data on area change
            RequestCharacterUpdate();
            AreaMatcher.Match(areaName, out _currentArea);

            _presence.Assets.SmallImageKey = _currentArea.Key;
            _presence.Timestamps = _settings.GetValOrDefault<bool>(SettingType.ShowElapsedTime) ? Timestamps.Now : null;
            _presence.State = null;
            PresenceUpdateSmallImageText();
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to login screen
        /// </summary>
        public void PresenceUpdateLoginScreen() {
            _character = null;
            _presence.Assets.SmallImageKey = null;
            _presence.Assets.SmallImageText = null;
            _presence.Assets.LargeImageKey = Misc.GetArtKey();
            _presence.Assets.LargeImageText = $"{Settings.ProgramName} {Settings.Version}";
            _presence.State = "Login screen";
            _presence.Details = null;
            _presence.Timestamps = _settings.GetValOrDefault<bool>(SettingType.ShowElapsedTime) ? Timestamps.Now : null;
            _hasUpdate = true;
        }

        /// <summary>
        /// Sets the presence to character select
        /// </summary>
        public void PresenceUpdateCharacterSelect() {
            _character = null;

            _presence.Assets.SmallImageKey = null;
            _presence.Assets.SmallImageText = null;
            _presence.Assets.LargeImageKey = Misc.GetArtKey();
            _presence.Assets.LargeImageText = $"{Settings.ProgramName} {Settings.Version}";
            _presence.State = "Character select";
            _presence.Details = null;
            _presence.Timestamps = _settings.GetValOrDefault<bool>(SettingType.ShowElapsedTime) ? Timestamps.Now : null;
            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence with the current character data
        /// </summary>
        private void PresenceUpdateCharacterData() {
            var largeAssetKey = Misc.GetArtKey(_character.Class);
            var xpPercent = Misc.GetPercentToNextLevel(_character.Level, _character.Experience);

            _presence.Assets.LargeImageKey = largeAssetKey;
            _presence.Details = _settings.GetValOrDefault<bool>(SettingType.ShowCharName)
                ? $"Playing as {_character.Name}"
                : $"Playing as a {_character.Class}";

            _presence.Assets.LargeImageText =
                (_settings.GetValOrDefault<bool>(SettingType.ShowCharLevel) ? $"Level {_character.Level} " : "") +
                _character.Class +
                (_settings.GetValOrDefault<bool>(SettingType.ShowCharXp) ? $" - {xpPercent}% xp" : "");

            PresenceUpdateSmallImageText();

            _hasUpdate = true;
        }

        /// <summary>
        /// Updates the presence small image text
        /// </summary>
        private void PresenceUpdateSmallImageText() {
            if (_currentArea != null && _presence.Assets.SmallImageKey != null) {
                _presence.Assets.SmallImageText = _character == null
                    ? $"{_currentArea.Name}"
                    : $"{_currentArea.Name} ({_character.League})";
            }
        }

        #endregion

        #region Pipe Connection Events

        private static void OnReady(object sender, ReadyMessage args) {
            //This is called when we are all ready to start receiving and sending discord events. 
            // It will give us some basic information about discord to use in the future.

            //It can be a good idea to send a initial presence update on this event too, just to setup the initial game state.
            Console.WriteLine(@"[EVENT] RPC Ready. Version: {0}", args.Version);
        }

        private static void OnClose(object sender, CloseMessage args) {
            //This is called when our client has closed. The client can no longer send or receive events after this message.
            // Connection will automatically try to re-establish and another OnReady will be called (unless it was disposed).
            Console.WriteLine(@"[EVENT] Lost Connection with client because of '{0}'", args.Reason);
        }

        private static void OnError(object sender, ErrorMessage args) {
            //Some error has occured from one of our messages. Could be a malformed presence for example.
            // Discord will give us one of these events and its upto us to handle it
            Console.WriteLine(@"[EVENT] Error occured within discord. ({1}) {0}", args.Message, args.Code);
        }

        private static void OnConnectionEstablished(object sender, ConnectionEstablishedMessage args) {
            //This is called when a pipe connection is established. The connection is not ready yet, but we have at least found a valid pipe.
            Console.WriteLine(@"[EVENT] Pipe Connection Established. Valid on pipe #{0}", args.ConnectedPipe);
        }

        private static void OnConnectionFailed(object sender, ConnectionFailedMessage args) {
            //This is called when the client fails to establish a connection to discord. 
            // It can be assumed that Discord is unavailable on the supplied pipe.
            Console.WriteLine(@"[EVENT] Pipe Connection Failed. Could not connect to pipe #{0}", args.FailedPipe);
        }

        #endregion
    }
}