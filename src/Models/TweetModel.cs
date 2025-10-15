namespace TweetModels
{
    public class Tweet
    {
        public int id { get; set; }
        public DateTime dataCreazione { get; set; }
        public string testo { get; set; } = "";
        public string idUtente { get; set; } = "";
        public string? immaginePost { get; set; } = "";
    }
}
