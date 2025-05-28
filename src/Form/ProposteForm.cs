using System.ComponentModel.DataAnnotations;

namespace Miciomania.Form.Proposte
{
    public class PropostaFormModel
    {
        [Required] public string Tipo { get; set; } = string.Empty;
        [Required] public string Nome { get; set; } = string.Empty;
        [Required] public string Descrizione { get; set; } = string.Empty;
        public string? Copertina { get; set; }
        [Required] public IFormFile File { get; set; } = null!;
        [Required] public string IdUtente { get; set; } = string.Empty;
    }

    public class PropostaPersonaleFormModel
    {
        [Required] public string IdUtente { get; set; } = string.Empty;
    }

}
