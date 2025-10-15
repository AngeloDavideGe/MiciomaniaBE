namespace TweetModels
{
    public class Tweet
    {
        public int id { get; set; }
        public DateTime datacreazione { get; set; }
        public string testo { get; set; } = "";
        public string idutente { get; set; } = "";
        public string? immaginepost { get; set; } = "";
    }
}
