using MiciomaniaNamespace.Interface;
using MiciomaniaNamespace.Models.Profilo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiciomaniaNamespace.Controllers
{
    [ApiController]
    [Route("api/home")]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CopyDbContext _contextCopy;

        public HomeController(AppDbContext context, CopyDbContext contextCopy)
        {
            _context = context;
            _contextCopy = contextCopy;
        }

        [HttpGet("get_profilo_by_id/{id_input}")]
        public async Task<ActionResult<Profilo>> GetProfiloUtente(string id_input)
        {
            Task<Utente?> utenteTask = _context.Utente.FirstOrDefaultAsync((u) => u.id == id_input);
            Task<List<Pubblicazioni>> pubblicazionniListTask = _contextCopy.Pubblicazioni
                .Where(m => m.idUtente == id_input)
                .Take(10)
                .ToListAsync();

            await Task.WhenAll(utenteTask, pubblicazionniListTask);

            Profilo result = new Profilo
            {
                Utente = utenteTask.Result ?? new Utente(),
                Pubblicazioni = pubblicazionniListTask.Result
            };

            return Ok(result);
        }

    }
}
