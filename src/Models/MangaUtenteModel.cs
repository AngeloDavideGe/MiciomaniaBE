namespace MangaUtenteModels
{
    public class MangaUtente
    {
        public string IdUtente { get; set; } = "";
        public string Preferiti { get; set; } = "";
        public string Letti { get; set; } = "";
        public string Completati { get; set; } = "";

        // public User User { get; set; } // navigazione
    }
}
