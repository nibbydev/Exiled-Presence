using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Domain;
using Service.Properties;

namespace Utility {
    public static class AreaMatcher {
        private static readonly Regex HideoutRegex = new Regex(@"^(.+) Hideout$");
        private static readonly Regex LabTrialRegex = new Regex("^Trial of [A-Za-z]+ [A-Za-z]+$");
        private static readonly List<Area[]> AreaData = new List<Area[]>(500);

        /// <summary>
        /// Load data in from JSON resource files
        /// </summary>
        static AreaMatcher() {
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.MapsWhite)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.MapsYellow)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.MapsRed)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.MapsUnique)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.AreaTown)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.AreaQuest)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.AreaVaal)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.AreaSpecial)));
            AreaData.Add(JsonUtility.Deserialize<Area[]>(Encoding.Default.GetString(Resources.AreaLab)));
        }
        
        /// <summary>
        /// Attempts to match an area name and find the art key
        /// </summary>
        public static bool Match(string areaName, out Area area) {
            if (HideoutRegex.IsMatch(areaName)) {
                area = new Area {Name = areaName, Key = "area_hideout"};
                return true;
            }
            
            // Try for an exact name match against the area data arrays.
            foreach (var areas in AreaData) {
                if ((area = areas.FirstOrDefault(t => t.Name.Equals(areaName))) != null) {
                    return true;
                }
            }

            if (LabTrialRegex.IsMatch(areaName)) {
                area = new Area {Name = areaName, Key = "area_trial"};
                return true;
            }

            // Default area
            area = new Area {
                Name = areaName,
                Key = "area_default"
            };
            
            return false;
        }
    }
}