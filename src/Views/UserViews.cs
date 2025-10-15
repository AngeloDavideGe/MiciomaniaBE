namespace UserViews
{
    public class UserParams
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string? profilePic { get; set; } = "";
        public string ruolo { get; set; } = "";
    }

    public class UserJoin
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string? profilePic { get; set; } = "";
        public string ruolo { get; set; } = "";
        public string? stato { get; set; } = "";
        public string squadra { get; set; } = "";
        public string? provincia { get; set; } = "";
        public int punteggio { get; set; }
        public string? bio { get; set; } = "";
        public string? telefono { get; set; } = "";
        public DateTime? compleanno { get; set; }
        public string? social { get; set; } = "";
    }
}
