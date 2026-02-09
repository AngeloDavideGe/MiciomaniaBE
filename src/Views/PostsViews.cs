using TweetModels;

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
        public UserPost user { get; set; }
        public List<Tweet> tweets { get; set; }

        public Profilo(UserPost u, List<Tweet> t)
        {
            user = u;
            tweets = t;
        }
    }

    public class UserPost
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string? profilePic { get; set; } = "";
        public string? stato { get; set; } = "";
        public string? provincia { get; set; } = "";
        public string? bio { get; set; } = "";
        public string? telefono { get; set; } = "";
        public DateTime? compleanno { get; set; }
        public Dictionary<string, string>? social { get; set; }
    }

}
