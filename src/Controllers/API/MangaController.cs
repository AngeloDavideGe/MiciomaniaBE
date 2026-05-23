using Microsoft.AspNetCore.Mvc;
using MangaViews;
using MangaModels;
using MangaForms;
using TaskOption;
using AppTask.Services;
using Manga.Services;

namespace Manga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController
    {
        private readonly MangaService _mangaService;
        private readonly AppTaskService _tasks;

        public MangaController(MangaService mangaService, AppTaskService tasks)
        {
            _mangaService = mangaService;
            _tasks = tasks;
        }

        [HttpGet("get_all_manga")]
        public async Task<ActionResult<List<MangaClass>>> GetAllManga()
        {
            return await _tasks.SingleTask(new SingleTaskOptions<List<MangaClass>>
            {
                Task = _mangaService.GetAllMangaCache,
                ErrorMessage = "Errore nel recupero dei manga"
            });
        }

        [HttpGet("get_manga_preferiti")]
        public async Task<ActionResult<MangaUtenteGet>> GetMangaPreferiti([FromQuery] string idUtente)
        {
            return await _tasks.SingleTask(new SingleTaskOptions<MangaUtenteGet>
            {
                Task = () => _mangaService.GetMangaUtente(idUtente),
                ErrorMessage = "Errore nel recupero dei manga"
            });
        }

        [HttpGet("get_all_manga_e_preferiti")]
        public async Task<ActionResult<MangaEPreferiti>> GetAllMangaEPreferiti([FromQuery] string idUtente)
        {
            return await _tasks.MultiTask(new MultiTaskOptions<List<MangaClass>, MangaUtenteGet, MangaEPreferiti>
            {
                Task1 = _mangaService.GetAllMangaCache,
                Task2 = () => _mangaService.GetMangaUtente(idUtente),
                ResultFactory = (manga, preferiti) => new MangaEPreferiti(manga, preferiti),
                ErrorMessage = "Errore nel recupero Manga e Preferiti"
            });
        }

        [HttpPost("post_manga")]
        public async Task<ActionResult> PostManga([FromBody] MangaClass mangaForm)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _mangaService.AggiungiManga(mangaForm),
                SuccessMessage = "Manga aggiunto con successo",
                ErrorMessage = "Errore inserimento manga"
            });
        }

        [HttpPut("upsert_manga_preferiti/{idUtente}")]
        public async Task<ActionResult> UpdateMangaPreferiti(string idUtente, [FromBody] MangaUtenteForm nuoviManga)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _mangaService.UpsertManga(idUtente, nuoviManga),
                SuccessMessage = $"Manga utente aggiornati per {idUtente}",
                ErrorMessage = "Errore aggiornamento manga utente"
            });
        }
    }
}
