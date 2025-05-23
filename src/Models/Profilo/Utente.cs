using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MiciomaniaNamespace.Models.Profilo
{
    public class Utente
    {
        [Key] public string? id { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? profilePic { get; set; }
        public string? ruolo { get; set; }
        public string? stato { get; set; }
        public string? team { get; set; }
        public string? provincia { get; set; }
        public string? citta { get; set; }
        public string? bio { get; set; }
        public string? telefono { get; set; }
        public DateTime? compleanno { get; set; }
        public JsonDocument? social { get; set; }
        public int? punteggio { get; set; }
    }

}