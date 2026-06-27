using Microsoft.AspNetCore.Mvc;
using InterazioniModels;
using InterazioniViews;
using PaginationForms;
using InterazioniForms;
using TaskOption;
using Interazioni.Services;
using Library.Service.TaskServices;
using Library.Service.BackGroundService;
using Cron.Services;
using CronModels;

namespace Interazioni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterazioniController
    {
        private readonly InterazioniService _interazioniService;
        private readonly BackGroundService _backgroundService;
        private readonly AppTaskService _task;

        public InterazioniController(
            InterazioniService interazioniService,
            BackGroundService backgroundService,
            AppTaskService taskService)
        {
            _interazioniService = interazioniService;
            _backgroundService = backgroundService;
            _task = taskService;
        }

        [HttpGet("get_all_interazioni")]
        public async Task<ActionResult<List<InterazioniGet>>> GetAllInterazioni()
        {
            return await _task.SingleTask(new SingleTaskOptions<List<InterazioniGet>>
            {
                Task = _interazioniService.GetAllInterazioni,
                ErrorMessage = "Errore nel recupero delle interazioni"
            });
        }

        [HttpGet("get_interazioni_by_id/{idUser}")]
        public async Task<ActionResult<List<InterazioniGet>>> GetInterazioniById(string idUser)
        {
            return await _task.SingleTask(new SingleTaskOptions<List<InterazioniGet>>
            {
                Task = () => _interazioniService.GetInterazioniById(idUser),
                ErrorMessage = "Errore nel recupero delle interazioni utente"
            });
        }

        [HttpGet("get_interazioni_paginate")]
        public async Task<ActionResult<PaginazioneOutput<InterazioniGet>>> GetInterazioniPaginate([FromQuery] PaginazioneInput input)
        {
            return await _task.MultiTask(new MultiTaskOptions<int, List<InterazioniDB>, PaginazioneOutput<InterazioniGet>>
            {
                Task1 = _interazioniService.TotaleInterazioni,
                Task2 = () => _interazioniService.GetInterazioniPaginate(input),
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
            ActionResult result = await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _interazioniService.UpsertInterazione(input),
                SuccessMessage = "Interazione aggiornata con successo",
                ErrorMessage = "Errore aggiornamento interazione"
            });

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    CronService cronService = sp.GetRequiredService<CronService>();

                    await cronService.PostUtentiCron(
                        idUtente: input.user1,
                        azione: $"Ha interagito con {input.user2}",
                        sezione: SezioneCron.Interazioni
                    );
                });
            }

            return result;
        }
    }
}