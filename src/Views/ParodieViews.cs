using CanzoniMiciomaniaModels;
using CanzoniUtenteModels;
using MangaMiciomaniaModels;
using MangaUtenteParModels;

namespace MangaViews
{
    public class AllaMangaParodie
    {
        public List<MangaMiciomania> mangaMiciomania { get; set; }
        public List<MangaUtentePar> mangaUtentePars { get; set; }

        public AllaMangaParodie(List<MangaMiciomania> l, List<MangaUtentePar> m)
        {
            mangaMiciomania = l;
            mangaUtentePars = m;
        }
    }

    public class AllaCanzoniParodie
    {
        public List<CanzoniMiciomania> canzoniMiciomania { get; set; }
        public List<CanzoniUtente> canzoniUtente { get; set; }

        public AllaCanzoniParodie(List<CanzoniMiciomania> l, List<CanzoniUtente> m)
        {
            canzoniMiciomania = l;
            canzoniUtente = m;
        }
    }
}
