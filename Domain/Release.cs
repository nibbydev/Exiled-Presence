namespace Domain {
    public sealed class Release {
        public string html_url { get; set; }
        public string tag_name { get; set; }
        public bool prerelease { get; set; }
    }
}