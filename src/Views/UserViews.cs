namespace UserViews
{
    public class UserParams
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string? profilePic { get; set; } = "";
        public string ruolo { get; set; } = "";
    }

    public class UserDto
    {
        public string id { get; set; } = "";
        public CredenzialiDto credenziali { get; set; } = new();
        public ProfileDto profile { get; set; } = new();
        public IscrizioneDto iscrizione { get; set; } = new();
        public AdminDto admin { get; set; } = new();
    }

    public class AdminDto
    {
        public string ruolo { get; set; } = "";
        public List<string> permessi { get; set; } = new List<string>();
    }

    public class CredenzialiDto
    {
        public string nome { get; set; } = "";
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string? profilePic { get; set; }
    }

    public class ProfileDto
    {
        public string? bio { get; set; }
        public DateTime? compleanno { get; set; }
        public Dictionary<string, string>? social { get; set; }
    }

    public class IscrizioneDto
    {
        public string? squadra { get; set; }
        public string? provincia { get; set; }
        public int? punteggio { get; set; }
    }
}
