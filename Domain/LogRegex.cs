using System;
using System.Text.RegularExpressions;

namespace Domain {
    public class LogRegex {
        public const string LogPrefix = @"^([0-9]{4}\/[0-9]{2}\/[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}).*\[INFO Client [0-9]*] :? ?";
        public Action<LogMatch> ParseAction;
        public LogType Type;
        public Regex[] RegExps;
    }
}