using System.ComponentModel.DataAnnotations;

namespace Forms.Proposte
{
    public class PropostaFormModel
    {
        [Required] public string Tipo { get; set; } = string.Empty;
        [Required] public string Nome { get; set; } = string.Empty;
        [Required] public string Descrizione { get; set; } = string.Empty;
        [Required] public string IdUtente { get; set; } = string.Empty;
        [Required] public string File { get; set; } = null!;
        public string? Copertina { get; set; }
    }

    public class PropostaPersonaleFormModel
    {
        [Required] public string IdUtente { get; set; } = string.Empty;
    }
}
