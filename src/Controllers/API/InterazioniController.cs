using Data.ApplicationDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InterazioniModels;
using InterazioniViews;
using PaginationForms;
using InterazioniForms;
using Microsoft.Extensions.Caching.Memory;
using TaskOption;

namespace Interazioni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterazioniController : UtilitiesController
    {
        public InterazioniController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_all_interazioni")]
        public async Task<ActionResult<List<InterazioniGet>>> GetAllInterazioni()
        {
            return await SingleTask(new SingleTaskOptions<List<InterazioniGet>>
            {
                Task = () => _context.Interazioni
                    .Select(i => new InterazioniGet
                    {
                        id = i.id,
                        user1 = i.user1,
                        user2 = i.user2,
                        conteggio = i.conteggio,
                        ultimoInvio = i.ultimo_invio
                    })
                    .ToListAsync(),

                ErrorMessage = "Errore nel recupero delle interazioni"
            });
        }

        [HttpGet("get_interazioni_by_id/{idUser}")]
        public async Task<ActionResult<List<InterazioniGet>>> GetInterazioniById(string idUser)
        {
            return await SingleTask(new SingleTaskOptions<List<InterazioniGet>>
            {
                Task = () => _context.Interazioni
                    .Where(i => i.user1 == idUser || i.user2 == idUser)
                    .Select(i => new InterazioniGet
                    {
                        id = i.id,
                        user1 = i.user1,
                        user2 = i.user2,
                        conteggio = i.conteggio,
                        ultimoInvio = i.ultimo_invio
                    })
                    .ToListAsync(),

                ErrorMessage = "Errore nel recupero delle interazioni utente"
            });
        }

        [HttpGet("get_interazioni_paginate")]
        public async Task<ActionResult<PaginazioneOutput<InterazioniGet>>> GetInterazioniPaginate([FromQuery] PaginazioneInput input)
        {
            IQueryable<InterazioniDB> query = _context.Interazioni.AsQueryable();

            query = input.order.ToLower() == "asc"
                ? query.OrderBy(e => EF.Property<object>(e, input.orderKey))
                : query.OrderByDescending(e => EF.Property<object>(e, input.orderKey));

            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await MultiTask(new MultiTaskOptions<int, List<InterazioniDB>, PaginazioneOutput<InterazioniGet>>
            {
                Task1 = () => newContext.Interazioni.CountAsync(),

                Task2 = () => query
                    .Skip((input.numPag - 1) * input.elemForPage)
                    .Take(input.elemForPage)
                    .ToListAsync(),

                ResultFactory = (totElem, interazioni) =>
                    new PaginazioneOutput<InterazioniGet>
                    {
                        elems = interazioni.Select(i => new InterazioniGet
                        {
                            id = i.id,
                            user1 = i.user1,
                            user2 = i.user2,
                            conteggio = i.conteggio,
                            ultimoInvio = i.ultimo_invio
                        })
                        .ToList(),

                        totElems = totElem
                    },

                ErrorMessage = "Errore nel recupero interazioni paginate"
            });
        }

        [HttpPut("upsert_interazione")]
        public async Task<ActionResult> UpsertInterazione([FromBody] InterazioniPut input)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
                {
                    InterazioniDB? interazioneEsistente = await _context.Interazioni
                        .FirstOrDefaultAsync(i =>
                            i.user1 == input.user1 &&
                            i.user2 == input.user2);

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
                },

                SuccessMessage = "Interazione aggiornata con successo",
                ErrorMessage = "Errore aggiornamento interazione"
            });
        }
    }
}