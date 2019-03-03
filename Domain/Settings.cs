namespace Domain {
    public class Settings {
        public string AccountName { get; set; }
        public string PoeSessionId { get; set; }

        public string GetObfuscatedSessId() {
            return PoeSessionId == null ? null : new string('*', 24) + PoeSessionId.Substring(24);
        }

        public void Update(Settings settings) {
            AccountName = settings.AccountName;
            PoeSessionId = settings.PoeSessionId;
        }
    }
}