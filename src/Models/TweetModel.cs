using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TweetModels
{
    public class Tweet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public DateTime dataCreazione { get; set; }
        public string testo { get; set; } = "";
        public string idUtente { get; set; } = "";
        public string? immaginePost { get; set; }
    }
}
