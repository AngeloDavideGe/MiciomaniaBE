namespace AdminModels
{
    public class Admin
    {
        public string idUtente { get; set; } = "";
        public string ruolo { get; set; } = "";
        public List<string> permessi { get; set; } = new List<string>();
    }
}
