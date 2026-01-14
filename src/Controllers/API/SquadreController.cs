using Microsoft.AspNetCore.Mvc;
using Data.ApplicationDbContext;
using GiocatoreModels;
using SquadreView;
using SquadraModels;
using Microsoft.EntityFrameworkCore;
using SquadreForms;

namespace Squadre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquadreController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SquadreController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        [HttpGet("get_squadre")]
        public async Task<ActionResult<List<SquadraView>>> GetSquadre()
        {
            try
            {
                List<SquadraView> squadre = await _context.Squadre
                    .Select((Squadra s) => new SquadraView
                    {
                        nome = s.nome,
                        punteggio = s.punteggio,
                        descrizione = s.descrizione,
                        colore = s.colore
                    })
                    .ToListAsync();

                return Ok(squadre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("get_squadre_e_giocatori")]
        public async Task<ActionResult<SquadreGiocatori>> GetSquadreGiocatori()
        {
            try
            {
                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    Task<List<SquadraView>> squadreTask = _context.Squadre
                        .Select((Squadra s) => new SquadraView
                        {
                            nome = s.nome,
                            punteggio = s.punteggio,
                            descrizione = s.descrizione,
                            colore = s.colore
                        })
                        .ToListAsync();

                    Task<List<Giocatore>> topGiocatoriTask = newContext.Giocatori
                        .Where((Giocatore g) => g.punteggio > 0)
                        .OrderByDescending((Giocatore g) => g.punteggio)
                        .Take(5)
                        .ToListAsync();

                    await Task.WhenAll(squadreTask, topGiocatoriTask);

                    SquadreGiocatori result = new SquadreGiocatori(squadreTask.Result, topGiocatoriTask.Result);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("update_punteggio_giocatore/{idUtente}")]
        public async Task<ActionResult> UpdatePunteggioGiocatore(string idUtente, [FromBody] SquadreUtenteForm squadreUtenteForm)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"SELECT squadre_schema.update_punteggio_giocatore(
                        {idUtente}, 
                        {squadreUtenteForm.nomeSquadra}, 
                        {squadreUtenteForm.punteggio}
                    )"
                );

                return Ok(new { message = "Punteggio del giocatore aggiornato con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}