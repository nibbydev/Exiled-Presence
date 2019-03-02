namespace Domain {
    public class Character {
        public string Name { get; set; }
        public string League { get; set; }
        public int ClassId { get; set; }
        public int AscendancyClass { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public uint Experience { get; set; }
        public bool? LastActive { get; set; }
    }
}