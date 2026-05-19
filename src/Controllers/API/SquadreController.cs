using Microsoft.AspNetCore.Mvc;
using Data.ApplicationDbContext;
using GiocatoreModels;
using SquadreView;
using SquadraModels;
using Microsoft.EntityFrameworkCore;
using SquadreForms;
using Squadre.Services;
using AppTask.Services;
using TaskOption;

namespace Squadre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquadreController
    {
        private readonly SquadreService _squadreService;
        private readonly AppTaskService _tasks;

        public SquadreController(SquadreService squadreService, AppTaskService tasks)
        {
            _squadreService = squadreService;
            _tasks = tasks;
        }

        [HttpGet("get_squadre")]
        public async Task<ActionResult<List<SquadraView>>> GetSquadre()
        {
            return await _tasks.SingleTask(new SingleTaskOptions<List<SquadraView>>
            {
                Task = _squadreService.GetSquadre,
                ErrorMessage = "Errore nel recupero delle squadre"
            });
        }

        [HttpGet("get_squadre_e_giocatori")]
        public async Task<ActionResult<SquadreGiocatori>> GetSquadreGiocatori()
        {
            return await _tasks.MultiTask(new MultiTaskOptions<List<SquadraView>, List<Giocatore>, SquadreGiocatori>
            {
                Task1 = _squadreService.GetSquadre,
                Task2 = _squadreService.GetTopGiocatori,
                ResultFactory = (squadre, giocatori) => new SquadreGiocatori(squadre, giocatori),
                ErrorMessage = "Errore nel recupero squadre e giocatori"
            });
        }

        [HttpPut("update_punteggio_giocatore/{idUtente}")]
        public async Task<ActionResult> UpdatePunteggioGiocatore(
            string idUtente,
            [FromBody] SquadreUtenteForm squadreUtenteForm)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _squadreService.UpdatePunteggioGiocatore(idUtente, squadreUtenteForm),
                ErrorMessage = "Errore nell'aggiornamento del punteggio del giocatore",
                SuccessMessage = "Punteggio del giocatore aggiornato con successo"
            });
        }
    }
}