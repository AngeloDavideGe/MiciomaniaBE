using System.Text.Json;
using AdminModels;
using CacheName;
using Data.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserForms;
using UserModels;
using UserViews;

namespace Utenti.Services
{
    public class UtentiService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly CacheService _cache;

        public UtentiService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            CacheService cache)
        {
            _context = context;
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public Task<List<UserParams>> GetAllUtenti()
        {
            return _cache.GetOrCreate(new TaskOption.CacheOptions<List<UserParams>>
            {
                NomeCache = "UtentiCache",
                DurataCache = TimeSpan.FromHours(2),
                Task = () => _context.Users
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
                    .ToListAsync()
            });
        }

        public async Task<UserDto?> GetUtenteByCredentials(string email, string password)
        {
            return await (
                from u in _context.Users
                where u.email == email && u.password == password

                join a in _context.Admins
                    on u.id equals a.idUtente into adminGroup
                from admin in adminGroup.DefaultIfEmpty()

                join g in _context.Giocatori
                    on u.id equals g.idUtente into giocatoreGroup
                from giocatore in giocatoreGroup.DefaultIfEmpty()

                select new UserDto
                {
                    id = u.id,
                    credenziali = new CredenzialiDto
                    {
                        nome = u.nome,
                        email = u.email,
                        password = u.password,
                        profilePic = u.profilePic,
                        ruolo = admin != null ? admin.ruolo : "user"
                    },
                    profile = new ProfileDto
                    {
                        bio = u.bio,
                        telefono = u.telefono,
                        compleanno = u.compleanno,
                        social = FormatSocial(u.social)
                    },
                    iscrizione = new IscrizioneDto
                    {
                        stato = u.stato,
                        squadra = giocatore != null ? giocatore.squadra : null,
                        provincia = u.provincia,
                        punteggio = giocatore != null ? giocatore.punteggio : null
                    }
                }
            ).FirstOrDefaultAsync();
        }

        public Task AggiungiUtente(UserPostForm userForm)
        {
            return _context.Database.ExecuteSqlRawAsync(
                "SELECT utenti_schema.create_user_complete({0}, {1}, {2}, {3})",
                userForm.username,
                userForm.nome,
                userForm.email,
                userForm.password
            );
        }

        public Task AggiornaUtente(string id, UserDto userForm)
        {
            var compleannoParam = new NpgsqlParameter(
                "compleanno",
                NpgsqlTypes.NpgsqlDbType.Timestamp
            )
            {
                Value = DateTime.SpecifyKind(userForm.profile.compleanno ?? DateTime.MinValue, DateTimeKind.Unspecified)
            };

            return _context.Database.ExecuteSqlInterpolatedAsync(
                $@"SELECT utenti_schema.update_user_complete(
                    {id},
                    {userForm.credenziali.nome},
                    {userForm.credenziali.email},
                    {userForm.credenziali.password},
                    {userForm.credenziali.profilePic},
                    {userForm.iscrizione.stato},
                    {userForm.iscrizione.provincia},
                    {userForm.profile.bio},
                    {userForm.profile.telefono},
                    {userForm.iscrizione.squadra},
                    {compleannoParam},
                    {JsonSerializer.Serialize(userForm.profile.social)}
                )"
            );
        }

        public Task EliminaUtente(string id)
        {
            return _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT utenti_schema.delete_user({id})"
            );
        }

        public Task AggiornaRuoloAdmin(string idUtente, UserRuoloUpdateForm ruolo)
        {
            return _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                    UPDATE utenti_schema.admin
                    SET ruolo = {ruolo.ruolo}
                    WHERE ""idUtente"" = {idUtente};
                "
            );
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