using Miciomania.Data;
using Miciomania.Form.Proposte;
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
        public async Task<List<Proposte>> GetProposte()
        {
            return await _context.Proposte.ToListAsync();
        }

        [HttpPost("invio_proposta")]
        public async Task<IActionResult> CreateProposta([FromForm] PropostaFormModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            MemoryStream memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            Proposte proposta = new Proposte
            {
                id_utente = model.IdUtente,
                tipo = model.Tipo,
                nome = model.Nome,
                descrizione = model.Descrizione,
                copertina = model.Copertina,
                file = fileBytes
            };

            _context.Proposte.Add(proposta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Proposta inserita con successo" });
        }

        [HttpDelete("elimina_proposta")]
        public async Task<IActionResult> DeleteProposta([FromQuery] PropostaPersonaleFormModel model)
        {
            Proposte? proposta = await _context.Proposte
                .FirstOrDefaultAsync(p => p.id_utente == model.IdUtente);

            if (proposta == null)
                return NotFound(new { message = "Proposta non trovata" });

            _context.Proposte.Remove(proposta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Proposta eliminata con successo" });
        }
    }
}
