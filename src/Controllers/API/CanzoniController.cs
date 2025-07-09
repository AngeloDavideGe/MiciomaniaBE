using Data.ApplicationDbContext;
using Models.Canzoni;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers.CanzoniController
{
    [ApiController]
    [Route("api/canzoni_miciomania")]
    public class CanzoniController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CanzoniController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("lista_canzoni")]
        public async Task<List<Canzone>> GetCanzoni()
        {
            return await _context.Canzone.ToListAsync();
        }
    }
}
