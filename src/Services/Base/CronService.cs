using CronModels;
using Data.ApplicationDbContext;
using Library.Extensions.Enumeratore;
using Microsoft.EntityFrameworkCore;

namespace Cron.Services
{
    public class CronService
    {
        private readonly AppDbContext _context;

        public CronService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CronUtenti>> GetUtentiCron(int maxElems)
        {
            return await _context.CronUtenti
                .OrderByDescending((CronUtenti cron) => cron.created_at)
                .Take(maxElems)
                .ToListAsync();
        }

        public async Task PostUtentiCron(string idUtente, string azione, SezioneCron sezione)
        {
            string sezioneString = sezione.GetNameEnum();

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                    INSERT INTO crono_schema.cron_utenti 
                    (""idUtente"", azione, sezione, created_at)
                    VALUES 
                    ({idUtente}, {azione}, {sezioneString}, {DateTime.UtcNow})
                "
            );
        }
    }
}