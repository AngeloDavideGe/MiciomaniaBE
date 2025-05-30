using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Miciomania.Models.Manga
{
    [Table("manga_miciomania")]
    public class Manga
    {
        [Key] public string id_autore { get; set; } = string.Empty;
        [Required] public string nome { get; set; } = string.Empty;
        [Required] public string genere { get; set; } = string.Empty;
        public string? copertina { get; set; }
        [Required] public string link { get; set; } = string.Empty;
    }
}
