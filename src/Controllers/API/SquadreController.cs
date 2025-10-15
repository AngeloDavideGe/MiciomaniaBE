using Microsoft.AspNetCore.Mvc;
using Data.ApplicationDbContext;
using GiocatoreModels;
using SquadreView;
using SquadraModels;
using Microsoft.EntityFrameworkCore;

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
                            punteggio = s.punteggio
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
        public async Task<ActionResult> UpdatePunteggioGiocatore(string idUtente, [FromBody] int nuovoPunteggio)
        {
            try
            {
                Giocatore? giocatore = await _context.Giocatori.FindAsync(idUtente);
                if (giocatore == null)
                {
                    return NotFound("Giocatore non trovato");
                }

                giocatore.punteggio += nuovoPunteggio;
                await _context.SaveChangesAsync();

                return Ok($"Ruolo admin aggiornato a: {giocatore.punteggio}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}