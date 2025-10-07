using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using UserModels;
using UserViews;
using AdminModels;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtentiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/utenti
        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllUtenti()
        {
            List<UserParams> utenti = await _context.Users
                .Join(
                    _context.Admins,
                    (User u) => u.id,
                    (Admin a) => a.idutente,
                    (User u, Admin a) => new UserParams
                    {
                        id = u.id,
                        nome = u.nome,
                        profilepic = u.profilepic,
                        ruolo = a.ruolo ?? "User"
                    }
                )
                .ToListAsync();

            return Ok(utenti);
        }

        // GET: api/utenti/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUtente(string id)
        {
            User? utente = await _context.Users.FindAsync(id);

            if (utente == null)
            {
                return NotFound();
            }

            return Ok(utente);
        }
    }
}
