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

        public class FavoritoRequest
        {
            public int BookId { get; set; }
            public int id { get; set; } // Para compatibilidad con el formato que envía el usuario
            public string? Titulo { get; set; }
            public string? Autor { get; set; }
            public string? Imagen { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] FavoritoRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Los datos del favorito son requeridos");
                }

                // Validar BookId (aceptar tanto BookId como id)
                if (request.BookId <= 0 && request.id <= 0)
                {
                    return BadRequest("BookId es requerido");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var userId = int.Parse(userIdClaim);

                // Usar el BookId correcto (priorizar request.BookId sobre request.id)
                var bookId = request.BookId > 0 ? request.BookId : request.id;

                // Verificar si ya está en favoritos
                var existente = await _context.Favoritos
                    .FirstOrDefaultAsync(f => f.UsuarioId == userId && f.BookId == bookId);

                if (existente != null)
                {
                    return BadRequest("El libro ya está en tus favoritos");
                }

                var favorito = new Favorito
                {
                    UsuarioId = userId,
                    BookId = bookId,
                    Titulo = request.Titulo ?? $"Libro {bookId}",
                    Autor = request.Autor ?? "Autor desconocido",
                    Imagen = request.Imagen ?? "/images/default-book.jpg"
                };

                _context.Favoritos.Add(favorito);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    id = favorito.Id,
                    message = "Libro agregado a favoritos correctamente" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
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