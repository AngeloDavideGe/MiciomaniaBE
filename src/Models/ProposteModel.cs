using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Miciomania.Models.Proposte
{
    [Table("proposte")]
    public class Proposta
    {
        [Key] public string id_autore { get; set; } = string.Empty;
        [Required] public string tipo { get; set; } = string.Empty;
        [Required] public string nome { get; set; } = string.Empty;
        [Required] public string genere { get; set; } = string.Empty;
        [Required] public string link { get; set; } = string.Empty;
        public string? copertina { get; set; } = string.Empty;
    }
}