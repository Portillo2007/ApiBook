using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BibliotecaAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(Favorito favorito)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            favorito.UsuarioId = int.Parse(userId);

            _context.Favoritos.Add(favorito);

            await _context.SaveChangesAsync();

            return Ok(favorito);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFavoritos()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoritos = await _context.Favoritos
                .Where(f => f.UsuarioId == int.Parse(userId))
                .ToListAsync();

            return Ok(favoritos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var favorito = await _context.Favoritos.FindAsync(id);

            if (favorito == null)
                return NotFound();

            _context.Favoritos.Remove(favorito);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}