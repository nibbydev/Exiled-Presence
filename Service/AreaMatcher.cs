using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Domain;

namespace Service {
    public static class AreaMatcher {
        private static readonly Regex HideoutRegex = new Regex(@"^(.+) Hideout$");
        private static readonly Regex LabTrialRegex = new Regex("^Trial of [A-Za-z]+ [A-Za-z]+$");
        private static readonly Regex LabAreaRegex = new Regex("^(Basilica|Domain|Estate|Mansion|Sanitorium|Sepulchre) (Annex|Atrium|Halls|Passage|Walkways|Crossing|Enclosure|Path)$");
        private static readonly Regex LabRoomRegex = new Regex("^Aspirant['s]{2} (Plaza|Trial)$");
        
        private static Dictionary<Area[], string> _artMap;
        private static Area[] _mapWhite, _mapYellow, _mapRed, _town, _quest, _vaal;

        public static bool Match(string areaName, out Area area, out string artKey) {
            // Default values
            artKey = "area_default";
            area = null;
            
            // Try for an exact area name match against the arrays
            area = _mapWhite.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_mapWhite, out artKey);
            area = _mapYellow.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_mapYellow, out artKey);
            area = _mapRed.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_mapRed, out artKey);
            area = _town.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_town, out artKey);
            area = _quest.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_quest, out artKey);
            area = _vaal.FirstOrDefault(t => t.Name.Equals(areaName));
            if (area != null) return _artMap.TryGetValue(_vaal, out artKey);
            

            if (HideoutRegex.IsMatch(areaName)) {
                artKey = "area_hideout";
                return true;
            }
            
            if (LabTrialRegex.IsMatch(areaName)) {
                artKey = "area_trial";
                return true;
            }
            
            if (LabAreaRegex.IsMatch(areaName) || LabRoomRegex.IsMatch(areaName)) {
                artKey = "area_lab";
                return true;
            }

            if (areaName.Equals("Azurite Mine")) {
                artKey = "area_delve";
                return true;
            }
            
            if (areaName.Equals("The Menagerie") || areaName.Equals("Menagerie Caverns")) {
                artKey = "area_bestiary";
                return true;
            }
            
            if (areaName.Equals("The Temple of Atzoatl")) {
                // todo: image
                return true;
            }

            return false;
        }


        public static void LoadStaticData() {
            var tmp = System.Text.Encoding.Default.GetString(Properties.Resources.maps_white);
            _mapWhite = JsonUtility.Deserialize<Area[]>(tmp);
            
            tmp = System.Text.Encoding.Default.GetString(Properties.Resources.maps_yellow);
            _mapYellow = JsonUtility.Deserialize<Area[]>(tmp);
            
            tmp = System.Text.Encoding.Default.GetString(Properties.Resources.maps_red);
            _mapRed = JsonUtility.Deserialize<Area[]>(tmp);

            tmp = System.Text.Encoding.Default.GetString(Properties.Resources.towns);
            _town = JsonUtility.Deserialize<Area[]>(tmp);
            
            tmp = System.Text.Encoding.Default.GetString(Properties.Resources.quest);
            _quest = JsonUtility.Deserialize<Area[]>(tmp);
            
            tmp = System.Text.Encoding.Default.GetString(Properties.Resources.vaal);
            _vaal = JsonUtility.Deserialize<Area[]>(tmp);
            
            _artMap = new Dictionary<Area[], string> {
                {_mapWhite, "area_map_white"},
                {_mapYellow, "area_map_yellow"},
                {_mapRed, "area_map_red"},
                {_town, "area_town"},
                {_quest, "area_quest"},
                {_vaal, "area_vaal"}
            };
        }
    }
}