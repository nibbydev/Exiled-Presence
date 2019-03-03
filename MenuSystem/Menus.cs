namespace MenuSystem {
    public static class Menus {
        public static readonly Menu AppStartFeedbackMenu = new Menu {
            Title = "Exiled Presence",
            Description = new[] {"Starting rich presence client..."}
        };

        public static readonly Menu AppStopFeedbackMenu = new Menu {
            Title = "Exiled Presence",
            Description = new[] {"Stopping rich presence client..."}
        };

        public static readonly Menu ConfigClearedFeedbackMenu = new Menu {
            Title = "Config Menu",
            Description = new[] {"Config cleared"}
        };

        public static readonly Menu SessIdInputMenu = new Menu {
            Title = "POESESSID Input",
            Description = new[] {
                "POESESSID is required if your profile is private. Without",
                "it the program cannot display your character's league, level,",
                "class, xp nor ascendancy class.",
                "",
                "To get your session id follow these steps:",
                "1. Open your browser and navigate to pathofexile.com",
                "2. Click the lock symbol left of the URL bar",
                "3. Navigate pathofexile.com > Cookies > POESESSID",
                "4. Copy what looks like '649a3c717ec06642db550046f1149b76'",
                "",
                "Warning: Do not share this string with anyone as that could",
                "give them access to some features of your account",
                "",
                "Please enter your POESESSID below"
            },
            IsInput = true
        };

        public static readonly Menu AccountNameInputMenu = new Menu {
            Title = "Account Name Input",
            Description = new[] {"Please enter your account name below"},
            IsInput = true
        };

        public static readonly Menu ConfigMenu = new Menu {
            Title = "Config Menu",
            Description = new[] {
                "Settings are saved to a config file and persist restarts.",
                "While the service is running, it must be restarted to apply",
                "any changes made."
            },
            MenuItems = new[] {
                new MenuItem {
                    Description = "Set account name",
                    Shortcut = "1",
                    MenuToRun = AccountNameInputMenu,
                    ValueDelegate = null
                },
                new MenuItem {
                    Description = "Set POESESSID",
                    Shortcut = "2",
                    MenuToRun = SessIdInputMenu,
                    ValueDelegate = null
                },
                new MenuItem {
                    Description = "Clear config",
                    Shortcut = "3",
                    MenuToRun = ConfigClearedFeedbackMenu,
                    ActionToExecute = null
                }
            }
        };

        public static readonly Menu MainMenu = new Menu {
            Title = "Exiled Presence",
            MenuItems = new[] {
                new MenuItem {
                    Description = "Run",
                    Shortcut = "1",
                    MenuToRun = AppStartFeedbackMenu,
                    ValueDelegate = null
                },
                new MenuItem {
                    Description = "Stop",
                    Shortcut = "2",
                    MenuToRun = AppStopFeedbackMenu
                },
                new MenuItem {
                    Description = "Config",
                    Shortcut = "3",
                    MenuToRun = ConfigMenu
                }
            }
        };

        public static readonly MenuItem GoBackItem = new MenuItem {
            Shortcut = "X",
            Description = "Go back"
        };

        public static readonly MenuItem ExitProgramItem = new MenuItem {
            Shortcut = "X",
            Description = "Exit program"
        };
    }
}