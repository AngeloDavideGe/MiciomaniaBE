using Microsoft.AspNetCore.Mvc;
using Data.ApplicationDbContext;
using GiocatoreModels;
using SquadreView;
using SquadraModels;
using Microsoft.EntityFrameworkCore;
using SquadreForms;
using Microsoft.Extensions.Caching.Memory;

namespace Squadre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquadreController : UtilitiesController
    {
        public SquadreController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_squadre")]
        public async Task<ActionResult<List<SquadraView>>> GetSquadre()
        {
            return await SingleTask(new SingleTaskOptions<List<SquadraView>>
            {
                Task = async () =>
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
                },

                ErrorMessage = "Errore nel recupero delle squadre"
            });
        }

        [HttpGet("get_squadre_e_giocatori")]
        public async Task<ActionResult<SquadreGiocatori>> GetSquadreGiocatori()
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await MultiTask(new MultiTaskOptions<List<SquadraView>, List<Giocatore>, SquadreGiocatori>
            {
                Task1 = () =>
                    _context.Squadre
                        .Select((Squadra s) => new SquadraView
                        {
                            nome = s.nome,
                            punteggio = s.punteggio,
                            descrizione = s.descrizione,
                            colore = s.colore
                        })
                        .ToListAsync(),

                Task2 = () =>
                    newContext.Giocatori
                        .Where((Giocatore g) => g.punteggio > 0)
                        .OrderByDescending((Giocatore g) => g.punteggio)
                        .Take(5)
                        .ToListAsync(),

                ResultFactory = (squadre, giocatori) => new SquadreGiocatori(squadre, giocatori),

                ErrorMessage = "Errore nel recupero squadre e giocatori"
            });
        }

        [HttpPut("update_punteggio_giocatore/{idUtente}")]
        public async Task<ActionResult> UpdatePunteggioGiocatore(
            string idUtente,
            [FromBody] SquadreUtenteForm squadreUtenteForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"
                        UPDATE squadre_schema.squadre
                        SET punteggio = punteggio + {squadreUtenteForm.punteggio}
                        WHERE nome = {squadreUtenteForm.nomeSquadra};

                        UPDATE squadre_schema.giocatori
                        SET punteggio = punteggio + {squadreUtenteForm.punteggio}
                        WHERE ""idUtente"" = {idUtente};
                    "
                ),
                ErrorMessage = "Errore nell'aggiornamento del punteggio del giocatore",
                SuccessMessage = "Punteggio del giocatore aggiornato con successo"
            });
        }
    }
}