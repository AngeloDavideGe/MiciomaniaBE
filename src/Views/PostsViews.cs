using TweetModels;
using UserModels;

namespace PostsViews
{
    public class TweetExtend
    {
        public int id { get; set; }
        public DateTime dataCreazione { get; set; }
        public string testo { get; set; } = "";
        public string idUtente { get; set; } = "";
        public string? immaginePost { get; set; } = "";
        public string? userProfilePic { get; set; }
        public string userName { get; set; } = "";
    }

    public class Profilo
    {
        public User user { get; set; }
        public List<Tweet> tweets { get; set; }

        public Profilo(User u, List<Tweet> t)
        {
            user = u;
            tweets = t;
        }
    }

}
