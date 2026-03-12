using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuscripcionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Suscripcion>>> Get()
        {
            return await _context.Suscripciones.ToListAsync();
        }

        [HttpPost]
public async Task<ActionResult<Suscripcion>> Crear(Suscripcion suscripcion)
{
    suscripcion.FechaInicio = DateTime.UtcNow;
    suscripcion.FechaFin = DateTime.UtcNow.AddMonths(1);
    suscripcion.Activa = true;

    _context.Suscripciones.Add(suscripcion);
    await _context.SaveChangesAsync();

    return Ok(suscripcion);
}

        [HttpPut("cancelar/{id}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var suscripcion = await _context.Suscripciones.FindAsync(id);

            if (suscripcion == null)
                return NotFound();

            suscripcion.Activa = false;
            suscripcion.FechaFin = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("activa/{usuarioId}")]
public async Task<IActionResult> TieneSuscripcionActiva(int usuarioId)
{
    var suscripcion = await _context.Suscripciones
        .Where(s => s.UsuarioId == usuarioId && s.Activa)
        .FirstOrDefaultAsync();

    if (suscripcion == null)
    {
        return Ok(new { suscrito = false });
    }

    return Ok(new { suscrito = true });
}
    }
}