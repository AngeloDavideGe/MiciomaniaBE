using Microsoft.AspNetCore.Mvc;
using GiocatoreModels;
using SquadreView;
using SquadreForms;
using Squadre.Services;
using Library.Service.TaskServices;
using TaskOption;
using Library.Service.BackGroundService;
using CronModels;

namespace Squadre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquadreController
    {
        private readonly SquadreService _squadreService;
        private readonly AppTaskService _tasks;
        private readonly BackGroundService _backgroundService;

        public SquadreController(SquadreService squadreService, AppTaskService tasks, BackGroundService backgroundService)
        {
            _squadreService = squadreService;
            _tasks = tasks;
            _backgroundService = backgroundService;
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
            ActionResult result = await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _squadreService.UpdatePunteggioGiocatore(idUtente, squadreUtenteForm),
                ErrorMessage = "Errore nell'aggiornamento del punteggio del giocatore",
                SuccessMessage = "Punteggio del giocatore aggiornato con successo"
            });

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    SquadreService squadreService = sp.GetRequiredService<SquadreService>();

                    await squadreService.PostUtentiCron(
                        idUtente: idUtente,
                        azione: $"Ha ottenuto {squadreUtenteForm.punteggio} punti ai mini-giochi",
                        sezione: SezioneCron.Games
                    );
                });
            }

            return result;
        }
    }
}