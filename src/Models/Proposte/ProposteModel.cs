using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Miciomania.Models.Proposte
{
    [Table("proposte")]
    public class Proposte
    {
        [Key] public string id_utente { get; set; } = string.Empty;
        [Required] public string tipo { get; set; } = string.Empty;
        [Required] public string nome { get; set; } = string.Empty;
        [Required] public string descrizione { get; set; } = string.Empty;
        public string? copertina { get; set; } = string.Empty;
        [Required] public byte[] file { get; set; } = Array.Empty<byte>();

        public Proposte() { }

        public Proposte(Proposte proposta)
        {
            id_utente = proposta.id_utente;
            tipo = proposta.tipo;
            nome = proposta.nome;
            descrizione = proposta.descrizione;
            copertina = proposta.copertina;
            file = proposta.file;
        }
    }

}
