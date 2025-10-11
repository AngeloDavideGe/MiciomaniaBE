using MangaModels;

namespace MangaViews
{
    public class MangaEPreferiti
    {
        // public List<Manga> listaManga { get; set; }
        public List<MangaClass> listaManga { get; set; }
        public MangaUtenteGet? mangaUtente { get; set; }

        public MangaEPreferiti(List<MangaClass> l, MangaUtenteGet? m)
        {
            this.listaManga = l;
            this.mangaUtente = m;
        }
    }

    public class MangaUtenteGet
    {
        public string preferiti { get; set; } = "";
        public string letti { get; set; } = "";
        public string completati { get; set; } = "";
    }
}
