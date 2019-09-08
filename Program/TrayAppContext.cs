using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Domain;
using Program.Properties;
using Service;

namespace Program {
    public class TrayAppContext : ApplicationContext {
        private readonly Domain.Settings _settings;
        private readonly Config _config;
        private readonly Controller _controller;
        private bool _openBrowserOnTooltipClick;
        private string _releaseUrl;

        // Icon that will appear in the tray
        private readonly NotifyIcon _trayItem = new NotifyIcon {
            Text = Domain.Settings.ProgramName,
            Icon = Resources.ico,
            BalloonTipTitle = Domain.Settings.ProgramName,
            ContextMenu = new ContextMenu(),
            Visible = true
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public TrayAppContext() {
            _settings = new Domain.Settings();
            _config = new Config(_settings);
            _controller = new Controller(_settings);
            
            // Construct a dynamic-ish context menu structure
            CreateContextMenuEntries();

            try {
                _config.Load();
                if (_settings.IsCheckUpdates()) CheckUpdates();
                _controller.Initialize();
            } catch (Exception e) {
                // Config has invalid options
                TooltipMsg(e.Message, "error");
            }
        }

        /// <summary>
        /// Attempts to read config and reinitialize the service
        /// </summary>
        private void Reload(object sender = null, EventArgs args = null) {
            _controller.Dispose();

            try {
                _config.Load();
                _controller.Initialize();
                TooltipMsg("Reload successful");
            } catch (ArgumentException e) {
                // Config has invalid options
                TooltipMsg(e.Message, "error");
            }
        }

        /// <summary>
        /// Constructs the tray menu structure
        /// </summary>
        private void CreateContextMenuEntries() {
            _trayItem.ContextMenu.MenuItems.Add("Config..", new[] {
                new MenuItem("Reload config", Reload),
                new MenuItem("Edit config", delegate { _config.OpenInEditor(); }),
                new MenuItem("Open in explorer", delegate { Misc.OpenPath(Config.CfgFolderPath); }),
                new MenuItem("Reset config", delegate {
                    _config.Reset();
                    TooltipMsg("Config reset");
                })
            });

            _trayItem.ContextMenu.MenuItems.Add("Startup..", new[] {
                new MenuItem("Add to startup", delegate {
                    TooltipMsg(File.Exists(Domain.Settings.StartupShortcutPath)
                        ? "Recreated startup shortcut"
                        : "Created startup shortcut", "info");
                    Win32.CreateShortcut(Domain.Settings.AppPath, Domain.Settings.StartupShortcutPath);
                }),
                new MenuItem("Remove from startup", delegate {
                    if (File.Exists(Domain.Settings.StartupShortcutPath)) {
                        File.Delete(Domain.Settings.StartupShortcutPath);
                        TooltipMsg("Deleted startup shortcut", "info");
                    } else TooltipMsg("Startup shortcut did not exist", "error");
                }),
                new MenuItem("Open in explorer", delegate { Misc.OpenPath(Domain.Settings.StartupFolderPath); })
            });

            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Check updates", delegate { CheckUpdates(true); })
            );

            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Exit", delegate {
                    // Hide the icon so it doesn't persist
                    _trayItem.Visible = false;
                    _controller.Dispose();
                    Application.Exit();
                })
            );

            // Register event handlers
            _trayItem.BalloonTipClicked += BalloonClick;
        }

        /// <summary>
        /// Displays a popup message to the user
        /// </summary>
        public void TooltipMsg(string ttMsg, string ttIcon = "none") {
            // Don't display empty messages
            if (string.IsNullOrEmpty(ttMsg)) {
                return;
            }

            // Attempt to map the string representation of ttIcon to its enum counterpart
            if (!Enum.TryParse(ttIcon, true, out ToolTipIcon ttIconEnum)) {
                return;
            }

            // Set values as display the tooltip
            _trayItem.BalloonTipIcon = ttIconEnum;
            _trayItem.BalloonTipText = ttMsg;
            _trayItem.ShowBalloonTip(0);
        }

        /// <summary>
        /// Checks for updates infrequently
        /// </summary>
        private async void CheckUpdates(bool verbose = false) {
            Console.WriteLine(@"Checking updates...");

            // Get latest release info
            var release = await Web.GetLatestRelease(Domain.Settings.ReleaseApi);
            
            // Save current time
            _settings.UpdateLastUpdateTime();
            _config.Save();
            
            if (release == null) {
                Console.WriteLine(@"No updates available");
                return;
            }

            if (Misc.IsNewVersion(Domain.Settings.Version, release.tag_name)) {
                _releaseUrl = release.html_url;
                _openBrowserOnTooltipClick = true;
                Console.WriteLine(@"New version is available");
                TooltipMsg($"{release.tag_name} released. Click here to open in browser");
            } else {
                Console.WriteLine(@"No updates available");
                if (verbose) TooltipMsg("No updates available");
            }
        }

        /// <summary>
        /// Callback for when user clicks on popup message
        /// </summary>
        private void BalloonClick(object sender, EventArgs args) {
            if (!_openBrowserOnTooltipClick) {
                return;
            }
            
            // Open the releases page in a new browser window
            Process.Start(new ProcessStartInfo(_releaseUrl));
            _openBrowserOnTooltipClick = false;
        }
    }
}