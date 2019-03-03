using System;
using System.Text;

namespace MenuSystem {
    public class MenuItem {
        public string Shortcut { get; set; }
        public string Description { get; set; }
        public bool IsDefaultChoice { get; set; }
        public bool ClearConsole { get; set; }
        public Menu MenuToRun { get; set; }
        public Action ActionToExecute { get; set; }
        public Func<string> ValueDelegate { get; set; }

        public override string ToString() {
            var sb = new StringBuilder();

            if (Shortcut == null) {
                sb.Append(Description);
            } else {
                sb.Append(Shortcut);
                sb.Append(") ");
                sb.Append(Description);

                if (ValueDelegate?.Invoke() != null) {
                    sb.Append(" (");
                    sb.Append(ValueDelegate());
                    sb.Append(")");
                }
            }
            
            return sb.ToString();
        }
    }
}