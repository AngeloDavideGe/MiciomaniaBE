using Data.ApplicationDbContext;
using InterazioniForms;
using InterazioniModels;
using InterazioniViews;
using Microsoft.EntityFrameworkCore;
using PaginationForms;

namespace Interazioni.Services
{
    public class InterazioniService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public InterazioniService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        public Task<List<InterazioniGet>> GetAllInterazioni()
        {
            return _context.Interazioni
                .Select((InterazioniDB i) => new InterazioniGet
                {
                    id = i.id,
                    user1 = i.user1,
                    user2 = i.user2,
                    conteggio = i.conteggio,
                    ultimoInvio = i.ultimo_invio
                })
                .ToListAsync();
        }

        public Task<List<InterazioniGet>> GetInterazioniById(string idUser)
        {
            return _context.Interazioni
                .Where((InterazioniDB i) => i.user1 == idUser || i.user2 == idUser)
                .Select((InterazioniDB i) => new InterazioniGet
                {
                    id = i.id,
                    user1 = i.user1,
                    user2 = i.user2,
                    conteggio = i.conteggio,
                    ultimoInvio = i.ultimo_invio
                })
                .ToListAsync();
        }

        public async Task<int> TotaleInterazioni()
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();
            return await newContext.Interazioni.CountAsync();
        }

        public async Task<List<InterazioniDB>> GetInterazioniPaginate(PaginazioneInput input)
        {
            IQueryable<InterazioniDB> query = _context.Interazioni.AsQueryable();

            query = input.order.ToLower() == "asc"
                ? query.OrderBy((InterazioniDB e) => EF.Property<object>(e, input.orderKey))
                : query.OrderByDescending((InterazioniDB e) => EF.Property<object>(e, input.orderKey));

            return await query
                .Skip((input.numPag - 1) * input.elemForPage)
                .Take(input.elemForPage)
                .ToListAsync();
        }

        public async Task UpsertInterazione(InterazioniPut input)
        {
            InterazioniDB? interazioneEsistente = await _context.Interazioni
                .FirstOrDefaultAsync((InterazioniDB i) =>
                    i.user1 == input.user1 &&
                    i.user2 == input.user2
                );

            if (interazioneEsistente != null)
            {
                interazioneEsistente.conteggio += 1;
                interazioneEsistente.ultimo_invio = DateTime.UtcNow;

                _context.Interazioni.Update(interazioneEsistente);
            }
            else
            {
                var nuovaInterazione = new InterazioniDB
                {
                    user1 = input.user1,
                    user2 = input.user2,
                    conteggio = 1,
                    ultimo_invio = DateTime.UtcNow
                };

                _context.Interazioni.Add(nuovaInterazione);
            }

            await _context.SaveChangesAsync();
        }
    }
}