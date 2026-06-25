using Microsoft.AspNetCore.Mvc;
using UserViews;
using UserForms;
using TaskOption;
using Utenti.Services;
using AppTask.Services;
using BackGroundName;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController
    {
        private readonly UtentiService _utentiService;
        private readonly BackGroundService _backgroundService;
        private readonly AppTaskService _task;

        public UtentiController(
          UtentiService utentiService,
          AppTaskService task,
          BackGroundService backgroundService
        )
        {
            _utentiService = utentiService;
            _task = task;
            _backgroundService = backgroundService;
        }

        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<List<UserParams>>> GetAllUtenti()
        {
            return await _task.SingleTask(new SingleTaskOptions<List<UserParams>>
            {
                Task = _utentiService.GetAllUtenti,
                ErrorMessage = "Errore recupero utenti"
            });
        }

        [HttpGet("get_utente_by_email")]
        public async Task<ActionResult<UserDto?>> GetUtenteByCredentials(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            ActionResult<UserDto?> result = await _task.SingleTask(new SingleTaskOptions<UserDto?>
            {
                Task = () => _utentiService.GetUtenteByCredentials(email, password),
                ErrorMessage = "Utente non trovato"
            });

            if (result.Result is OkObjectResult okResult && okResult.Value is UserDto user)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    UtentiService utentiService = sp.GetRequiredService<UtentiService>();

                    await utentiService.PostUtentiCron(
                        idUtente: user.id,
                        azione: "Ha effettuato il Login",
                        sezione: "Profilo"
                    );
                });
            }

            return result;
        }

        [HttpPost("post_utente")]
        public async Task<ActionResult> PostUser([FromBody] UserPostForm userForm)
        {
            return await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _utentiService.AggiungiUtente(userForm),
                SuccessMessage = "Utente aggiunto con successo",
                ErrorMessage = "Errore creazione utente"
            });
        }

        [HttpPut("update_utente/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserDto userForm)
        {
            return await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _utentiService.AggiornaUtente(id, userForm),
                SuccessMessage = "Utente aggiornato con successo",
                ErrorMessage = "Errore aggiornamento utente"
            });
        }

        [HttpDelete("delete_utente/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            return await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _utentiService.EliminaUtente(id),
                SuccessMessage = "Utente eliminato con successo",
                ErrorMessage = "Errore eliminazione utente"
            });
        }

        [HttpPut("update_ruolo_admin/{idUtente}")]
        public async Task<ActionResult> UpdateRuoloAdmin(string idUtente, [FromBody] UserRuoloUpdateForm ruoloForm)
        {
            _backgroundService.FireAndForget(async sp =>
            {
                UtentiService utentiService = sp.GetRequiredService<UtentiService>();

                await utentiService.PostUtentiCron(
                    idUtente: idUtente,
                    azione: $"Ha aggiornato il suo ruolo in {ruoloForm.ruolo}",
                    sezione: "Admin"
                );
            });

            return await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _utentiService.AggiornaRuoloAdmin(idUtente, ruoloForm),
                SuccessMessage = "Ruolo aggiornato con successo",
                ErrorMessage = "Errore aggiornamento ruolo"
            });
        }
    }
}
