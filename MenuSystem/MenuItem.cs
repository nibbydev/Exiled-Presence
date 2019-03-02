using System;

namespace MenuSystem {
    public class MenuItem {
        public string Shortcut { get; set; }
        public string Description { get; set; }
        public bool IsDefaultChoice { get; set; }
        public bool ClearConsole { get; set; }
        public Menu MenuToRun { get; set; }
        public Action ActionToExecute { get; set; }

        public override string ToString() {
            return Shortcut == null ? Description : $"{Shortcut}) {Description}";
        }
    }
}