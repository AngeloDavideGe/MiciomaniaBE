using MiciomaniaNamespace.Models;

namespace MiciomaniaNamespace.Interface
{
    public class AllManga
    {
        public List<ListaManga> ListaManga { get; set; } = new();
        public List<MangaUtente> MangaUtente { get; set; } = new();
    }
}
