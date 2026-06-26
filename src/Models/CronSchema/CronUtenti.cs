namespace CronModels
{
    public class CronUtenti
    {
        public int id { get; set; }
        public string idUtente { get; set; } = "";
        public string azione { get; set; } = "";
        public DateTime created_at { get; set; }
        public string sezione { get; set; } = "";
    }

    public enum SezioneCron
    {
        Manga,
        Posts,
        Admin,
        Games,
        Profilo
    }
}
