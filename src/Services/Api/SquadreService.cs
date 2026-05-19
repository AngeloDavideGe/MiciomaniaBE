using Data.ApplicationDbContext;
using GiocatoreModels;
using Microsoft.EntityFrameworkCore;
using SquadraModels;
using SquadreForms;
using SquadreView;

namespace Squadre.Services
{
    public class SquadreService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SquadreService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        public Task<List<SquadraView>> GetSquadre()
        {
            return _context.Squadre
                .Select((Squadra s) => new SquadraView
                {
                    nome = s.nome,
                    punteggio = s.punteggio,
                    descrizione = s.descrizione,
                    colore = s.colore
                })
                .ToListAsync();
        }

        public Task<List<Giocatore>> GetTopGiocatori()
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return newContext.Giocatori
                .Where((Giocatore g) => g.punteggio > 0)
                .OrderByDescending((Giocatore g) => g.punteggio)
                .Take(5)
                .ToListAsync();
        }

        public Task UpdatePunteggioGiocatore(
            string idUtente,
            SquadreUtenteForm squadreUtenteForm)
        {
            return _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                    UPDATE squadre_schema.squadre
                    SET punteggio = punteggio + {squadreUtenteForm.punteggio}
                    WHERE nome = {squadreUtenteForm.nomeSquadra};

                    UPDATE squadre_schema.giocatori
                    SET punteggio = punteggio + {squadreUtenteForm.punteggio}
                    WHERE ""idUtente"" = {idUtente};
                "
            );
        }
    }
}