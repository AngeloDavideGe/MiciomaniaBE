using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Miciomania.Models.Proposte
{
    [Table("proposte")]
    public class Proposta
    {
        [Key] public string id_utente { get; set; } = string.Empty;
        [Required] public string tipo { get; set; } = string.Empty;
        [Required] public string nome { get; set; } = string.Empty;
        [Required] public string descrizione { get; set; } = string.Empty;
        [Required] public string file { get; set; } = string.Empty;
        public string? copertina { get; set; } = string.Empty;
    }
}