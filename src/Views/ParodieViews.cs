using CanzoniMiciomaniaModels;
using CanzoniUtenteModels;
using MangaMiciomaniaModels;
using MangaUtenteParModels;

namespace MangaViews
{
    public class AllMangaParodie
    {
        public List<MangaMiciomania> mangaMiciomania { get; set; }
        public List<MangaUtentePar> mangaUtentePars { get; set; }

        public AllMangaParodie(List<MangaMiciomania> l, List<MangaUtentePar> m)
        {
            mangaMiciomania = l;
            mangaUtentePars = m;
        }
    }

    public class AllCanzoniParodie
    {
        public List<CanzoniMiciomania> canzoniMiciomania { get; set; }
        public List<CanzoniUtente> canzoniUtente { get; set; }

        public AllCanzoniParodie(List<CanzoniMiciomania> l, List<CanzoniUtente> m)
        {
            canzoniMiciomania = l;
            canzoniUtente = m;
        }
    }

    public class MangaECanzoneUtente
    {
        public MangaUtentePar mangaUtente { get; set; }
        public CanzoniUtente canzoniUtente { get; set; }

        public MangaECanzoneUtente(MangaUtentePar l, CanzoniUtente m)
        {
            mangaUtente = l;
            canzoniUtente = m;
        }
    }
}
