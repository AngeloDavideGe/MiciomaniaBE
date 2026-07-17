using MangaMiciomaniaModels;
using MangaModels;

namespace MangaViews
{
    public class MangaEPreferiti
    {
        public List<MangaClass> listaManga { get; set; }
        public List<MangaMiciomania> micioManga { get; set; }
        public MangaUtenteGet? mangaUtente { get; set; }

        public MangaEPreferiti(List<MangaClass> lm, List<MangaMiciomania> mm, MangaUtenteGet? mu)
        {
            listaManga = lm;
            micioManga = mm;
            mangaUtente = mu;
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
