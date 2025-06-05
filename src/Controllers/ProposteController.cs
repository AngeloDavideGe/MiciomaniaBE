using Miciomania.Data.ApplicationDbContext;
using Miciomania.Forms.Proposte;
using Miciomania.Models.Proposte;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers.ProposteController
{
    [ApiController]
    [Route("api/proposte")]
    public class ProposteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProposteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("lista_proposte")]
        public async Task<List<Proposta>> GetProposte()
        {
            return await _context.Proposta.ToListAsync();
        }

        [HttpPost("invio_proposta")]
        public async Task<IActionResult> CreateProposta([FromForm] PropostaFormModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Proposta proposta = new Proposta
            {
                id_autore = model.IdUtente,
                tipo = model.Tipo,
                nome = model.Nome,
                genere = model.Descrizione,
                copertina = model.Copertina,
                link = model.File
            };

            _context.Proposta.Add(proposta);
            await _context.SaveChangesAsync();

            return Ok(proposta);
        }

        [HttpDelete("elimina_proposta")]
        public async Task<IActionResult> DeleteProposta([FromQuery] PropostaPersonaleFormModel model)
        {
            Proposta? proposta = await _context.Proposta
                .FirstOrDefaultAsync(p => p.id_autore == model.IdUtente);

            if (proposta == null)
                return NotFound(new { message = "Proposta non trovata" });

            _context.Proposta.Remove(proposta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Proposta eliminata con successo" });
        }
    }
}
