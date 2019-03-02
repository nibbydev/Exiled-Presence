using System.Text.RegularExpressions;

namespace Domain {
    public class LogMatch {
        public string Msg { get; set; }
        public Match Match { get; set; }
        public LogType Type { get; set; }
    }
}