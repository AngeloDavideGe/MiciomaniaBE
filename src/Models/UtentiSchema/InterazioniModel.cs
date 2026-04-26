using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterazioniModels
{
    public class InterazioniDB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string user1 { get; set; } = "";
        public string user2 { get; set; } = "";
        public int conteggio { get; set; }
        public DateTime ultimo_invio { get; set; }
    }
}
