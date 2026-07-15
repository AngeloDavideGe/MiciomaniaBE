using Microsoft.AspNetCore.Mvc;
using Library.Service.TaskServices;
using TaskOption;
using CronModels;
using Cron.Services;

namespace Crono.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CronoController
    {
        private readonly CronService _cronService;
        private readonly AppTaskService _tasks;

        public CronoController(CronService cronService, AppTaskService tasks)
        {
            _tasks = tasks;
            _cronService = cronService;
        }

        [HttpGet("get_notifiche")]
        public async Task<ActionResult<List<CronUtenti>>> GetNotifiche([FromQuery] int maxElems)
        {
            return await _tasks.SingleTask(new SingleTaskOptions<List<CronUtenti>>
            {
                Task = () => _cronService.GetUtentiCron(maxElems),
                ErrorMessage = "Errore nel recupero delle notifiche"
            });
        }
    }
}