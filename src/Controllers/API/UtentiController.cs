using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using UserModels;
using UserViews;
using AdminModels;
using UserForms;
using GiocatoreModels;

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

        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllUtenti()
        {
            List<UserParams> utenti = await _context.Users
                .Join(
                    _context.Admins,
                    (User u) => u.id,
                    (Admin a) => a.idUtente,
                    (User u, Admin a) => new UserParams
                    {
                        id = u.id,
                        nome = u.nome,
                        profilePic = u.profilePic,
                        ruolo = a.ruolo ?? "User"
                    }
                )
                .ToListAsync();

            return Ok(utenti);
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
                        squadra = temp.user.squadra,
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

                return Ok("Utente aggiornato con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("update_utente/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] User userForm)
        {
            try
            {
                User? existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    return NotFound("Utente non trovato");
                }

                existingUser.nome = userForm.nome;
                existingUser.email = userForm.email;
                existingUser.password = userForm.password;
                existingUser.profilePic = userForm.profilePic;
                existingUser.stato = userForm.stato;
                existingUser.squadra = userForm.squadra;
                existingUser.provincia = userForm.provincia;
                existingUser.bio = userForm.bio;
                existingUser.telefono = userForm.telefono;
                existingUser.compleanno = userForm.compleanno;
                existingUser.social = userForm.social;

                await _context.SaveChangesAsync();
                return Ok("Utente aggiornato con successo");
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

                return Ok("Utente eliminato con successo");
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

                return Ok($"Ruolo admin aggiornato a: {nuovoRuolo}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
