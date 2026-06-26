using CronModels;
using Data.ApplicationDbContext;
using GiocatoreModels;
using Library.Extensions.Enumeratore;
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

        public async Task<List<SquadraView>> GetSquadre()
        {
            return await _context.Squadre
                .Select((Squadra s) => new SquadraView
                {
                    nome = s.nome,
                    punteggio = s.punteggio,
                    descrizione = s.descrizione,
                    colore = s.colore
                })
                .ToListAsync();
        }

        public async Task<List<Giocatore>> GetTopGiocatori()
        {
            await using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await newContext.Giocatori
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

        public async Task PostUtentiCron(string idUtente, string azione, SezioneCron sezione)
        {
            string sezioneString = sezione.GetNameEnum();

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                    INSERT INTO crono_schema.cron_utenti 
                    (""idUtente"", azione, sezione, created_at)
                    VALUES 
                    ({idUtente}, {azione}, {sezioneString}, {DateTime.UtcNow})
                "
            );
        }
    }
}