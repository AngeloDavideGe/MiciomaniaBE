using System.ComponentModel.DataAnnotations;

namespace MiciomaniaNamespace.Models.Profilo
{
    public class Pubblicazioni
    {
        [Key] public int id { get; set; }
        public DateTime? dataCreazione { get; set; }
        public string? testo { get; set; }
        public string? idUtente { get; set; }
    }

}