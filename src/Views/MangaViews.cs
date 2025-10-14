using MangaModels;

namespace MangaViews
{
    public class MangaEPreferiti
    {
        public List<MangaClass> listaManga { get; set; }
        public MangaUtenteGet? mangaUtente { get; set; }

        public MangaEPreferiti(List<MangaClass> l, MangaUtenteGet? m)
        {
            listaManga = l;
            mangaUtente = m;
        }
    }

    public class MangaUtenteGet
    {
        public string preferiti { get; set; }
        public string letti { get; set; }
        public string completati { get; set; }

        public MangaUtenteGet(string p, string l, string c)
        {
            preferiti = p;
            letti = l;
            completati = c;
        }
    }
}
