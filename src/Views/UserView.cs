using Models.Canzoni;
using Models.Manga;
using Models.Proposte;

namespace Views.UserView
{
    public class ElementiUser
    {
        public Manga Manga { get; set; } = new Manga();
        public Canzone Canzone { get; set; } = new Canzone();
        public Proposta Proposta { get; set; } = new Proposta();
    }
}