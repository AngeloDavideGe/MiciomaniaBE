using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using UserModels;
using UserViews;
using AdminModels;
using UserForms;
using GiocatoreModels;
using Npgsql;
using Microsoft.Extensions.Caching.Memory;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
        private readonly string _utentiCacheKey = "AllUtentiCache";

        public UtentiController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllUtenti()
        {
            List<UserParams>? utenti = await _cache.GetOrCreateAsync(_utentiCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                entry.SetPriority(CacheItemPriority.Normal);

                return await _context.Users
                    .Join(
                        _context.Admins,
                        (User u) => u.id,
                        (Admin a) => a.idUtente,
                        (User u, Admin a) => new UserParams
                        {
                            id = u.id,
                            nome = u.nome,
                            profilePic = u.profilePic,
                            ruolo = a.ruolo
                        }
                    )
                    .ToListAsync();
            });

            return Ok(utenti ?? new List<UserParams>());
        }

        [HttpGet("get_utente_by_email")]
        public async Task<ActionResult<UserJoin>> GetUtenteByCredentials(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            UserJoin? utenteJoin = await _context.Users
                .Where(u => u.email == email && u.password == password)
                .Join(
                    _context.Admins,
                    (User user) => user.id,
                    (Admin admin) => admin.idUtente,
                    (User user, Admin admin) => new { user, admin }
                )
                .Join(
                    _context.Giocatori,
                    (temp) => temp.user.id,
                    (Giocatore giocatore) => giocatore.idUtente,
                    (temp, giocatore) => new UserJoin
                    {
                        id = temp.user.id,
                        nome = temp.user.nome,
                        email = temp.user.email,
                        password = temp.user.password,
                        profilePic = temp.user.profilePic,
                        ruolo = temp.admin.ruolo,
                        stato = temp.user.stato,
                        squadra = giocatore.squadra,
                        provincia = temp.user.provincia,
                        punteggio = giocatore.punteggio,
                        bio = temp.user.bio,
                        telefono = temp.user.telefono,
                        compleanno = temp.user.compleanno,
                        social = temp.user.social
                    }
                )
                .FirstOrDefaultAsync();

            if (utenteJoin == null)
            {
                return NotFound();
            }

            return Ok(utenteJoin);
        }

        [HttpPost("post_utente")]
        public async Task<ActionResult> PostUser([FromBody] UserPostForm userForm)
        {
            User? existingUser = await _context.Users
                .FirstOrDefaultAsync((User u) => u.id == userForm.nome);

            if (existingUser != null) return Conflict("Username già esistente");

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT utenti_schema.create_user_complete({0}, {1}, {2}, {3})",
                    userForm.username, userForm.nome, userForm.email, userForm.password
                );

                List<UserParams>? utentiInCache = _cache.Get<List<UserParams>>(_utentiCacheKey);
                if (utentiInCache != null)
                {
                    utentiInCache.Add(new UserParams
                    {
                        id = userForm.username,
                        nome = userForm.nome,
                        profilePic = null,
                        ruolo = "user"
                    });

                    _cache.Set(_utentiCacheKey, utentiInCache, _cacheDuration);
                }

                return Ok("Utente aggiornato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("update_utente/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserUpdate userForm)
        {
            try
            {
                var compleannoParam = new NpgsqlParameter("compleanno", NpgsqlTypes.NpgsqlDbType.Timestamp)
                {
                    Value = DateTime.SpecifyKind(userForm.compleanno, DateTimeKind.Unspecified)
                };

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"SELECT utenti_schema.update_user_complete(
                        {id}, 
                        {userForm.nome}, 
                        {userForm.email}, 
                        {userForm.password}, 
                        {userForm.profilePic}, 
                        {userForm.stato}, 
                        {userForm.provincia}, 
                        {userForm.bio}, 
                        {userForm.telefono}, 
                        {userForm.squadra}, 
                        {compleannoParam}
                    )"
                );

                List<UserParams>? utentiInCache = _cache.Get<List<UserParams>>(_utentiCacheKey);
                if (utentiInCache != null)
                {
                    UserParams? utenteDaAggiornare = utentiInCache.FirstOrDefault((UserParams u) => u.id == id);
                    if (utenteDaAggiornare != null)
                    {
                        utenteDaAggiornare.nome = userForm.nome;
                        utenteDaAggiornare.profilePic = userForm.profilePic;
                    }

                    _cache.Set(_utentiCacheKey, utentiInCache, _cacheDuration);
                }

                return Ok(new { message = "Utente aggiornato con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpDelete("delete_utente/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("Utente non trovato");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                List<UserParams>? utentiInCache = _cache.Get<List<UserParams>>(_utentiCacheKey);
                if (utentiInCache != null)
                {
                    utentiInCache.RemoveAll((UserParams u) => u.id == id);
                    _cache.Set(_utentiCacheKey, utentiInCache, _cacheDuration);
                }

                return Ok(new { message = "Utente eliminato con successo" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Impossibile eliminare l'utente per vincoli di integrità referenziale");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("update_ruolo_admin/{idUtente}")]
        public async Task<ActionResult> UpdateRuoloAdmin(string idUtente, [FromBody] string nuovoRuolo)
        {
            try
            {
                Admin? admin = await _context.Admins.FindAsync(idUtente);
                if (admin == null)
                {
                    return NotFound("Admin non trovato");
                }

                admin.ruolo = nuovoRuolo;
                await _context.SaveChangesAsync();

                List<UserParams>? utentiInCache = _cache.Get<List<UserParams>>(_utentiCacheKey);
                if (utentiInCache != null)
                {
                    UserParams? utenteDaAggiornare = utentiInCache.FirstOrDefault((UserParams u) => u.id == idUtente);
                    if (utenteDaAggiornare != null)
                    {
                        utenteDaAggiornare.ruolo = nuovoRuolo;
                    }

                    _cache.Set(_utentiCacheKey, utentiInCache, _cacheDuration);
                }

                return Ok(new { message = "Ruolo aggiornato con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
