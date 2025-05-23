using MiciomaniaNamespace.Models.Profilo;

namespace MiciomaniaNamespace.Interface
{
    public class Profilo
    {
        public Utente Utente { get; set; } = new();
        public List<Pubblicazioni> Pubblicazioni { get; set; } = new();
    }
}
