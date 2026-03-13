using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BibliotecaAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HistorialController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistorial()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var historial = await _context.HistorialLibros
                .Where(h => h.UsuarioId == int.Parse(userId))
                .OrderByDescending(h => h.FechaLectura)
                .ToListAsync();

            return Ok(historial);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetHistorialReciente(int limit = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var historial = await _context.HistorialLibros
                .Where(h => h.UsuarioId == int.Parse(userId))
                .OrderByDescending(h => h.FechaLectura)
                .Take(limit)
                .ToListAsync();

            return Ok(historial);
        }
    }
}
