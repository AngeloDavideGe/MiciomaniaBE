using Microsoft.AspNetCore.Mvc;
using MangaUtenteParModels;
using MangaViews;
using CanzoniUtenteModels;
using ParodieForms;
using MangaMiciomaniaModels;
using CanzoniMiciomaniaModels;
using TaskOption;
using Parodie.Services;
using AppTask.Services;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParodieController
    {
        private readonly ParodieService _parodieService;
        private readonly AppTaskService _tasks;

        public ParodieController(ParodieService ParodieService, AppTaskService tasks)
        {
            _parodieService = ParodieService;
            _tasks = tasks;
        }

        [HttpGet("get_all_manga_parodia")]
        public async Task<ActionResult<AllMangaParodie>> GetAllMangaParodia()
        {
            return await _tasks.MultiTask(new MultiTaskOptions<List<MangaMiciomania>, List<MangaUtentePar>, AllMangaParodie>
            {
                Task1 = _parodieService.GetAllMangaMicio,
                Task2 = _parodieService.GetAllMangaUtente,
                ResultFactory = (mangaMicio, mangaUtente) => new AllMangaParodie(mangaMicio, mangaUtente),
                ErrorMessage = "Errore recupero manga parodia"
            });
        }

        [HttpGet("get_all_canzoni_parodia")]
        public async Task<ActionResult<AllCanzoniParodie>> GetAllCanzoniParodia()
        {
            return await _tasks.MultiTask(new MultiTaskOptions<List<CanzoniMiciomania>, List<CanzoniUtente>, AllCanzoniParodie>
            {
                Task1 = _parodieService.GetAllCanzoniMicio,
                Task2 = _parodieService.GetAllCanzoniUtente,
                ResultFactory = (canzoniMicio, canzoniUtente) => new AllCanzoniParodie(canzoniMicio, canzoniUtente),
                ErrorMessage = "Errore recupero canzoni parodia"
            });
        }

        [HttpGet("get_manga_e_canzone_utente/{idUtente}")]
        public async Task<ActionResult<MangaECanzoneUtente>> GetMangaECanzoneUtente(string idUtente)
        {
            return await _tasks.MultiTask(new MultiTaskOptions<MangaUtentePar, CanzoniUtente, MangaECanzoneUtente>
            {
                Task1 = () => _parodieService.GetMangatente(idUtente),
                Task2 = () => _parodieService.GetCanzoneUtente(idUtente),
                ResultFactory = (mangaUtente, canzoneUtente) => new MangaECanzoneUtente(mangaUtente, canzoneUtente),
                ErrorMessage = "Errore recupero parodie utente"
            });
        }

        [HttpPut("upsert_manga_o_canzone/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] ParodieUtenteForm parodieForm)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _parodieService.updateParodieUtente(id, parodieForm),
                SuccessMessage = "Parodia aggiornata con successo",
                ErrorMessage = "Errore update parodia"
            });
        }
    }
}
