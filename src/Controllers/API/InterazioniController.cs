using Data.ApplicationDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InterazioniModels;
using InterazioniViews;
using PaginationForms;
using Microsoft.Extensions.Caching.Memory;
using InterazioniForms;

namespace Interazioni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterazioniController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;


        public InterazioniController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        [HttpGet("get_all_interazioni")]
        public async Task<ActionResult<List<InterazioniGet>>> GetAllInterazioni()
        {
            try
            {
                List<InterazioniGet>? interazioniList = await _context.Interazioni.Select(i => new InterazioniGet
                {
                    id = i.id,
                    user1 = i.user1,
                    user2 = i.user2,
                    conteggio = i.conteggio,
                    ultimoInvio = i.ultimo_invio
                }).ToListAsync();

                return Ok(interazioniList ?? new List<InterazioniGet>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("get_interazioni_by_id/{idUser}")]
        public async Task<ActionResult<List<InterazioniGet>>> GetInterazioniById(string idUser)
        {
            try
            {
                List<InterazioniGet> interazioniUtente = await _context.Interazioni
                .Where(i => i.user1 == idUser || i.user2 == idUser)
                .Select(i => new InterazioniGet
                {
                    id = i.id,
                    user1 = i.user1,
                    user2 = i.user2,
                    conteggio = i.conteggio,
                    ultimoInvio = i.ultimo_invio
                })
                .ToListAsync();

                return Ok(interazioniUtente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }

        }

        [HttpGet("get_interazioni_paginate")]
        public async Task<ActionResult<PaginazioneOutput<InterazioniGet>>> GetInterazioniPaginate([FromQuery] PaginazioneInput input)
        {
            try
            {

                IQueryable<InterazioniDB>? query = _context.Interazioni.AsQueryable();

                if (input.order.ToLower() == "asc")
                {
                    query = query.OrderBy((InterazioniDB e) => EF.Property<object>(e, input.orderKey));
                }
                else
                {
                    query = query.OrderByDescending((InterazioniDB e) => EF.Property<object>(e, input.orderKey));
                }

                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {

                    Task<int> totaleElementiTask = newContext.Interazioni.CountAsync();

                    Task<List<InterazioniDB>> interazioniTask = query
                        .Skip((input.numPag - 1) * input.elemForPage)
                        .Take(input.elemForPage)
                        .ToListAsync();

                    await Task.WhenAll(totaleElementiTask, interazioniTask);

                    int totaleElementi = totaleElementiTask.Result;

                    PaginazioneOutput<InterazioniGet> output = new PaginazioneOutput<InterazioniGet>
                    {
                        elems = interazioniTask.Result.Select(i => new InterazioniGet
                        {
                            id = i.id,
                            user1 = i.user1,
                            user2 = i.user2,
                            conteggio = i.conteggio,
                            ultimoInvio = i.ultimo_invio
                        }).ToList(),
                        totElems = totaleElementi,
                        totPags = (int)Math.Ceiling(totaleElementi / (double)input.elemForPage)
                    };

                    return Ok(output);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("upsert_interazione")]
        public async Task<ActionResult> UpsertInterazione([FromBody] InterazioniPut input)
        {
            try
            {
                InterazioniDB? interazioneEsistente = await _context.Interazioni
                    .FirstOrDefaultAsync((InterazioniDB i) => (i.user1 == input.user1 && i.user2 == input.user2));

                if (interazioneEsistente != null)
                {
                    interazioneEsistente.conteggio += 1;
                    interazioneEsistente.ultimo_invio = DateTime.UtcNow;

                    _context.Interazioni.Update(interazioneEsistente);
                }
                else
                {
                    InterazioniDB nuovaInterazione = new InterazioniDB
                    {
                        user1 = input.user1,
                        user2 = input.user2,
                        conteggio = 1,
                        ultimo_invio = DateTime.UtcNow
                    };
                    _context.Interazioni.Add(nuovaInterazione);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}