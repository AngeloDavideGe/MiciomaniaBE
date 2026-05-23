using CanzoniMiciomaniaModels;
using CanzoniUtenteModels;
using Data.ApplicationDbContext;
using MangaMiciomaniaModels;
using MangaUtenteParModels;
using Microsoft.EntityFrameworkCore;
using ParodieForms;

namespace Parodie.Services
{
    public class ParodieService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ParodieService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        public async Task<List<MangaMiciomania>> GetAllMangaMicio()
        {
            return await _context.MangaMicio.ToListAsync();
        }

        public async Task<List<MangaUtentePar>> GetAllMangaUtente()
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await newContext.MangaUserPar.ToListAsync();
        }

        public async Task<List<CanzoniMiciomania>> GetAllCanzoniMicio()
        {
            return await _context.CanzoniMicio.ToListAsync();
        }

        public async Task<List<CanzoniUtente>> GetAllCanzoniUtente()
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await newContext.CanzoniUser.ToListAsync();
        }

        public async Task<MangaUtentePar> GetMangatente(string idUtente)
        {
            return await _context.MangaUserPar
                .Where((MangaUtentePar m) => m.idUtente == idUtente)
                .FirstOrDefaultAsync()
                .ContinueWith((Task<MangaUtentePar?> task) => task.Result ?? new MangaUtentePar());
        }

        public async Task<CanzoniUtente> GetCanzoneUtente(string idUtente)
        {
            await using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await newContext.CanzoniUser
                .Where((CanzoniUtente m) => m.idUtente == idUtente)
                .FirstOrDefaultAsync()
                .ContinueWith((Task<CanzoniUtente?> task) => task.Result ?? new CanzoniUtente());
        }

        public Task updateParodieUtente(string id, ParodieUtenteForm parodieForm)
        {
            return _context.Database.ExecuteSqlInterpolatedAsync(
                $@"SELECT parodie_schema.insert_manga_o_canzone(
                    {id}, 
                    {parodieForm.nome}, 
                    {parodieForm.genere}, 
                    {parodieForm.copertina}, 
                    {parodieForm.url}, 
                    {parodieForm.tipo}
                )"
            );
        }
    }
}