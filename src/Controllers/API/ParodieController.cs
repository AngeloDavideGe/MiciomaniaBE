using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaUtenteParModels;
using MangaViews;
using CanzoniUtenteModels;
using ParodieForms;
using MangaMiciomaniaModels;
using CanzoniMiciomaniaModels;
using Microsoft.Extensions.Caching.Memory;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParodieController : UtilitiesController
    {
        public ParodieController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_all_manga_parodia")]
        public async Task<ActionResult<AllMangaParodie>> GetAllMangaParodia()
        {
            using (AppDbContext newContext = _contextFactory.CreateDbContext())
            {
                return await MultiTask(new MultiTaskOptions<List<MangaMiciomania>, List<MangaUtentePar>, AllMangaParodie>
                {
                    Task1 = () => _context.MangaMicio.ToListAsync(),
                    Task2 = () => newContext.MangaUserPar.ToListAsync(),
                    ResultFactory = (mangaMicio, mangaUtente) => new AllMangaParodie(mangaMicio, mangaUtente),
                    ErrorMessage = "Errore recupero manga parodia"
                }
                );
            }
        }

        [HttpGet("get_all_canzoni_parodia")]
        public async Task<ActionResult<AllCanzoniParodie>> GetAllCanzoniParodia()
        {
            using (AppDbContext newContext = _contextFactory.CreateDbContext())
            {
                return await MultiTask(new MultiTaskOptions<List<CanzoniMiciomania>, List<CanzoniUtente>, AllCanzoniParodie>
                {
                    Task1 = () => _context.CanzoniMicio.ToListAsync(),
                    Task2 = () => newContext.CanzoniUser.ToListAsync(),
                    ResultFactory = (canzoniMicio, canzoniUtente) => new AllCanzoniParodie(canzoniMicio, canzoniUtente),
                    ErrorMessage = "Errore recupero canzoni parodia"
                }
                );
            }
        }

        [HttpGet("get_manga_e_canzone_utente/{idUtente}")]
        public async Task<ActionResult<MangaECanzoneUtente>> GetMangaECanzoneUtente(string idUtente)
        {

            using (AppDbContext newContext = _contextFactory.CreateDbContext())
            {
                return await MultiTask(new MultiTaskOptions<MangaUtentePar?, CanzoniUtente?, MangaECanzoneUtente>
                {
                    Task1 = () => _context.MangaUserPar
                        .Where((MangaUtentePar m) => m.idUtente == idUtente)
                        .FirstOrDefaultAsync(),

                    Task2 = () => newContext.CanzoniUser
                        .Where((CanzoniUtente c) => c.idUtente == idUtente)
                        .FirstOrDefaultAsync(),

                    ResultFactory = (mangaUtente, canzoneUtente) => new MangaECanzoneUtente(
                        mangaUtente ?? new MangaUtentePar(),
                        canzoneUtente ?? new CanzoniUtente()
                    ),

                    ErrorMessage = "Errore recupero parodie utente"
                }
                );
            }
        }

        [HttpPut("upsert_manga_o_canzone/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] ParodieUtenteForm parodieForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"SELECT parodie_schema.insert_manga_o_canzone(
                        {id}, 
                        {parodieForm.nome}, 
                        {parodieForm.genere}, 
                        {parodieForm.copertina}, 
                        {parodieForm.url}, 
                        {parodieForm.tipo}
                    )"
                ),
                SuccessMessage = "Parodia aggiornata con successo",
                ErrorMessage = "Errore update parodia"
            });
        }
    }
}
