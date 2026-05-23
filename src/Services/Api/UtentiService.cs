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

        public async Task<UserJoin?> GetUtenteByCredentials(string email, string password)
        {
            return await _context.Users
                .Where((User u) => u.email == email && u.password == password)
                .Join(
                    _context.Admins,
                    (User u) => u.id,
                    (Admin a) => a.idUtente,
                    (User u, Admin a) => new { u, a }
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
                .FirstOrDefaultAsync();
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

        public Task AggiornaUtente(string id, UserUpdate userForm)
        {
            var compleannoParam = new NpgsqlParameter(
                "compleanno",
                NpgsqlTypes.NpgsqlDbType.Timestamp
            )
            {
                Value = DateTime.SpecifyKind(userForm.compleanno, DateTimeKind.Unspecified)
            };

            return _context.Database.ExecuteSqlInterpolatedAsync(
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
                    SET ruolo = {ruolo}
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