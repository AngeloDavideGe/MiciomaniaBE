using GiocatoreModels;

namespace SquadreView
{
    public class SquadreGiocatori
    {
        public List<SquadraView> squadre { get; set; }
        public List<Giocatore> giocatori { get; set; }

        public SquadreGiocatori(List<SquadraView> s, List<Giocatore> g)
        {
            squadre = s;
            giocatori = g;
        }
    }

    public class SquadraView
    {
        public string nome { get; set; } = "";
        public int punteggio { get; set; }
    }
}