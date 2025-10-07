namespace TweetModels
{
    public class Tweet
    {
        public int Id { get; set; }
        public DateTime DataCreazione { get; set; }
        public string Testo { get; set; } = "";
        public string IdUtente { get; set; } = "";
        public string ImmaginePost { get; set; } = "";

        // public User User { get; set; } // navigazione
    }
}
