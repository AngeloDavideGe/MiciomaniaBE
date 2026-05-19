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
using System.Text.Json;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController : UtilitiesController
    {
        public UtentiController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllUtenti()
        {
            List<UserParams> utenti = await CacheFunc(new CacheOptions<List<UserParams>>
            {
                NomeCache = "UtentiCache",
                DurataCache = TimeSpan.FromHours(2),
                Task = () => _context.Users
                    .Join(
                        _context.Admins,
                        u => u.id,
                        a => a.idUtente,
                        (u, a) => new UserParams
                        {
                            id = u.id,
                            nome = u.nome,
                            profilePic = u.profilePic,
                            ruolo = a.ruolo
                        }
                    )
                    .ToListAsync()
            });

            return Ok(utenti);
        }

        [HttpGet("get_utente_by_email")]
        public async Task<ActionResult<UserJoin?>> GetUtenteByCredentials(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            return await SingleTask(new SingleTaskOptions<UserJoin?>
            {
                Task = () => _context.Users
                    .Where(u => u.email == email && u.password == password)
                    .Join(
                        _context.Admins,
                        u => u.id,
                        a => a.idUtente,
                        (u, a) => new { u, a }
                    )
                    .Join(
                        _context.Giocatori,
                        x => x.u.id,
                        g => g.idUtente,
                        (x, g) => new UserJoin
                        {
                            id = x.u.id,
                            nome = x.u.nome,
                            email = x.u.email,
                            password = x.u.password,
                            profilePic = x.u.profilePic,
                            ruolo = x.a.ruolo,
                            stato = x.u.stato,
                            squadra = g.squadra,
                            provincia = x.u.provincia,
                            punteggio = g.punteggio,
                            bio = x.u.bio,
                            telefono = x.u.telefono,
                            compleanno = x.u.compleanno,
                            social = FormatSocial(x.u.social)
                        }
                    )
                    .FirstOrDefaultAsync(),

                ErrorMessage = "Utente non trovato"
            });
        }

        [HttpPost("post_utente")]
        public async Task<ActionResult> PostUser([FromBody] UserPostForm userForm)
        {
            User? existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.id == userForm.nome);

            if (existingUser != null)
                return Conflict("Username già esistente");

            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlRawAsync(
                    "SELECT utenti_schema.create_user_complete({0}, {1}, {2}, {3})",
                    userForm.username,
                    userForm.nome,
                    userForm.email,
                    userForm.password
                ),
                SuccessMessage = "Utente aggiunto con successo",
                ErrorMessage = "Errore creazione utente"
            });
        }

        [HttpPut("update_utente/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserUpdate userForm)
        {
            var compleannoParam = new NpgsqlParameter(
                "compleanno",
                NpgsqlTypes.NpgsqlDbType.Timestamp
            )
            {
                Value = DateTime.SpecifyKind(userForm.compleanno, DateTimeKind.Unspecified)
            };

            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlInterpolatedAsync(
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
                        {compleannoParam},
                        {JsonSerializer.Serialize(userForm.social)}
                    )"
                ),
                SuccessMessage = "Utente aggiornato con successo",
                ErrorMessage = "Errore aggiornamento utente"
            });
        }

        [HttpDelete("delete_utente/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT utenti_schema.delete_user({id})"
                ),
                SuccessMessage = "Utente eliminato con successo",
                ErrorMessage = "Errore eliminazione utente"
            });
        }

        [HttpPut("update_ruolo_admin/{idUtente}")]
        public async Task<ActionResult> UpdateRuoloAdmin(string idUtente, [FromBody] UserRuoloUpdateForm ruoloForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = () => _context.Database.ExecuteSqlRawAsync(
                    @"
                        UPDATE utenti_schema.admin
                        SET ruolo = {0}
                        WHERE ""idUtente"" = {1};
                    ",
                    ruoloForm.ruolo,
                    idUtente
                ),
                SuccessMessage = "Ruolo aggiornato con successo",
                ErrorMessage = "Errore aggiornamento ruolo"
            });
        }

        private static Dictionary<string, string>? FormatSocial(JsonElement? socialElement)
        {
            if (socialElement == null || !socialElement.HasValue || socialElement.Value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(socialElement.Value);
            }
            catch
            {
                return null;
            }
        }
    }

}
