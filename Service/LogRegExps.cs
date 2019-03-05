using System.Text.RegularExpressions;
using Domain;

namespace Service {
    public static class LogRegExps {
        /// <summary>
        /// I stole some of these from https://github.com/klayveR/poe-log-monitor
        /// </summary>
        public static readonly LogRegex[] RegExpList = {
            new LogRegex {
                Type = LogType.LoginScreen,
                ParseAction = null,
                RegExps = new[] {
                    // 2017/12/27 18:38:37 16952332 698 [INFO Client 5156] Abnormal disconnect: An unexpected disconnection occurred.
                    new Regex(LogRegex.LogPrefix + @"Abnormal disconnect.+$"),
                    // 2019/03/01 13:19:48 876619328 2cb [INFO Client 14380] Finished checking files
                    new Regex(LogRegex.LogPrefix + @"Finished checking files$")
                }
            },
            
            new LogRegex {
                Type = LogType.CharacterSelect,
                ParseAction = null,
                RegExps = new[] {
                    // 2019/03/01 12:22:13 873163875 ac [INFO Client 4364] Async connecting to lon01.login.pathofexile.com:20481
                    // new Regex(LogRegex.LogPrefix + @"Async connecting.+$"),
                    // 2019/03/01 13:23:54 876865125 d3 [INFO Client 14380] Connected to lon01.login.pathofexile.com in 0ms.
                    new Regex(LogRegex.LogPrefix + @"Connected to [a-z]+[0-9]*\.login\.pathofexile\.com in [0-9]*ms\.$")
                }
            },
            
            new LogRegex {
                Type = LogType.AreaChange,
                ParseAction = null,
                RegExps = new[] {
                    // 2019/03/01 12:36:42 874033015 a21 [INFO Client 4364] : You have entered Oriath.
                    new Regex(LogRegex.LogPrefix + @"You have entered (.*)\.$")
                }
            },
            
            new LogRegex {
                Type = LogType.StatusChange,
                ParseAction = null,
                RegExps = new[] {
                    // 2019/03/01 12:39:58 874229375 a21 [INFO Client 4364] : AFK mode is now ON. Autoreply "This player is AFK."
                    // 2019/03/01 12:40:06 874236671 a21 [INFO Client 4364] : DND mode is now ON. Autoreply "This player has DND mode enabled."
                    // 2019/03/01 12:40:09 874240437 a21 [INFO Client 4364] : DND mode is now OFF.
                    // 2019/03/01 12:40:01 874231687 a21 [INFO Client 4364] : AFK mode is now OFF.
                    new Regex(LogRegex.LogPrefix + "(DND|AFK) mode is now (ON|OFF)\\.( Autoreply \"(.*)\")?$")
                }
            }
        };
    }
}