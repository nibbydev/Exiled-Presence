namespace Domain {
    public class Settings {
        public string AccountName { get; set; }
        public string PoeSessionId { get; set; }

        public void Update(Settings settings) {
            AccountName = settings.AccountName;
            PoeSessionId = settings.PoeSessionId;
        }
    }
}