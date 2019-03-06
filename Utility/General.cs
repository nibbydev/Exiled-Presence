using System;
using System.Linq;

namespace Utility {
    public static class General {
        /// <summary>
        /// Xp array. If level is 2, then the value at position 2 is the total amount of xp required to reach lvl 2
        /// </summary>
        private static readonly uint[] Xp = {
            0, 0, 525, 1760, 3781, 7184, 12186, 19324, 29377, 43181, 61693, 85990, 117506, 157384, 207736, 269997,
            346462, 439268, 551295, 685171, 843709, 1030734, 1249629, 1504995, 1800847, 2142652, 2535122, 2984677,
            3496798, 4080655, 4742836, 5490247, 6334393, 7283446, 8384398, 9541110, 10874351, 12361842, 14018289,
            15859432, 17905634, 20171471, 22679999, 25456123, 28517857, 31897771, 35621447, 39721017, 44225461,
            49176560, 54607467, 60565335, 67094245, 74247659, 82075627, 90631041, 99984974, 110197515, 121340161,
            133497202, 146749362, 161191120, 176922628, 194049893, 212684946, 232956711, 255001620, 278952403,
            304972236, 333233648, 363906163, 397194041, 433312945, 472476370, 514937180, 560961898, 610815862,
            664824416, 723298169, 786612664, 855129128, 929261318, 1009443795, 1096169525, 1189918242, 1291270350,
            1400795257, 1519130326, 1646943474, 1784977296, 1934009687, 2094900291, 2268549086, 2455921256, 2658074992,
            2876116901, 3111280300, 3364828162, 3638186694, 3932818530, 4250334444
        };

        /// <summary>
        /// Returns asset filename based on ascendancy class
        /// </summary>
        public static string GetArtKey(string @class = null) {
            switch (@class) {
                case "Duelist":
                case "Templar":
                case "Shadow":
                case "Scion":
                case "Ranger":
                case "Marauder":
                case "Witch":
                    return "class_" + @class.ToLower();
                case "Champion":
                case "Gladiator":
                case "Slayer":
                case "Berserker":
                case "Chieftain":
                case "Juggernaut":
                case "Deadeye":
                case "Pathfinder":
                case "Raider":
                case "Ascendant":
                case "Assassin":
                case "Saboteur":
                case "Trickster":
                case "Guardian":
                case "Hierophant":
                case "Inquisitor":
                case "Elementalist":
                case "Necromancer":
                case "Occultist":
                    return "ascendancy_" + @class.ToLower();
                default:
                    return "misc_logo";
            }
        }

        /// <summary>
        /// Returns the base class associated with the specified ascendancy class
        /// </summary>
        public static string GetBaseClass(string @class) {
            switch (@class) {
                case "Champion":
                case "Gladiator":
                case "Slayer":
                    return "Duelist";

                case "Berserker":
                case "Chieftain":
                case "Juggernaut":
                    return "Marauder";

                case "Deadeye":
                case "Pathfinder":
                case "Raider":
                    return "Ranger";

                case "Ascendant":
                    return "Scion";

                case "Assassin":
                case "Saboteur":
                case "Trickster":
                    return "Shadow";

                case "Guardian":
                case "Hierophant":
                case "Inquisitor":
                    return "Templar";

                case "Elementalist":
                case "Necromancer":
                case "Occultist":
                    return "Witch";

                default:
                    return @class;
            }
        }

        /// <summary>
        /// Get the % of xp required to the next level
        /// </summary>
        public static int GetPercentToNextLevel(int lvl, uint xp) {
            if (lvl < 0 || lvl > 100 || xp > Xp.Last()) {
                throw new ArgumentException();
            }

            var currentLvlXp = Xp[lvl];
            var nextLvlXp = Xp[lvl == 100 ? 100 : lvl + 1];
            return (int) Math.Floor((xp - currentLvlXp) / (double) (nextLvlXp - currentLvlXp) * 100f);
        }
    }
}