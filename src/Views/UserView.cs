using Miciomania.Models.Canzoni;
using Miciomania.Models.Manga;
using Miciomania.Models.Proposte;

namespace Miciomania.Views.UserView
{
    public class ElementiUser
    {
        public Manga Manga { get; set; } = new Manga();
        public Canzone Canzone { get; set; } = new Canzone();
        public Proposta Proposta { get; set; } = new Proposta();
    }
}