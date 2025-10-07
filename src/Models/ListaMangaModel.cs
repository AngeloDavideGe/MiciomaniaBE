namespace ListaMangaModels
{
    public class ListaManga
    {
        public int Id { get; set; } // PK auto-increment
        public string Nome { get; set; } = "";
        public string Autore { get; set; } = "";
        public string Genere { get; set; } = "";
        public string Copertina { get; set; } = "";
        public string Path { get; set; } = "";
        public bool Completato { get; set; }
    }
}
