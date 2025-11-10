namespace MangaModels
{
    public class MangaClass
    {
        public int id { get; set; }
        public string nome { get; set; } = "";
        public string autore { get; set; } = "";
        public string genere { get; set; } = "";
        public string copertina { get; set; } = "";
        public string path { get; set; } = "";
        public bool completato { get; set; }
    }
}
