using System.ComponentModel.DataAnnotations;

namespace MiciomaniaNamespace.Models.Manga
{
    public class MangaUtente
    {
        [Key] public string? id_utente { get; set; }
        public string? manga_preferiti { get; set; }
        public string? manga_letti { get; set; }
        public string? manga_completati { get; set; }
    }
}
