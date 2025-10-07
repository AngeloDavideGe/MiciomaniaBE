namespace UserModels
{
    public class User
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ProfilePic { get; set; } = "";
        public string Stato { get; set; } = "";
        public string squadra { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Citta { get; set; } = "";
        public int? Punteggio { get; set; }
        public string Bio { get; set; } = "";
        public string Telefono { get; set; } = "";
        public DateTime? Compleanno { get; set; }
        public string Social { get; set; } = "";
    }
}
