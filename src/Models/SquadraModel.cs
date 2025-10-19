using System.ComponentModel.DataAnnotations;

namespace SquadraModels
{
    public class Squadra
    {
        [Key] public int id { get; set; }
        public string nome { get; set; } = "";
        public int punteggio { get; set; }
    }
}
