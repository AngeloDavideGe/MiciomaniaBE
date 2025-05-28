using Miciomania.Models.Manga;
using Miciomania.Models.Proposte;

namespace Interfaces.UserInterfaces
{
    public class ElementiUser
    {
        public Manga Manga { get; set; } = new Manga();
        public Proposte Proposta { get; set; } = new Proposte();
    }
}