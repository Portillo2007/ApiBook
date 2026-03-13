using BibliotecaAPI.Data;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly GutenbergService _gutenberg;
        private readonly ApplicationDbContext _context;

        public BooksController(GutenbergService gutenberg, ApplicationDbContext context)
        {
            _gutenberg = gutenberg;
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            var books = await _gutenberg.SearchBooks(query);
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var libro = await _gutenberg.ObtenerLibro(id);
            if (libro == null)
                return NotFound();
            return Ok(libro);
        }

        [Authorize]
        [HttpGet("read/{bookId}")]
        public async Task<IActionResult> LeerLibro(int bookId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Usuario no autenticado");
            }

            var userId = int.Parse(userIdClaim);

            // Verificar si el usuario tiene suscripción activa O permitir acceso básico
            var suscripcion = await _context.Suscripciones
                .Where(s => s.UsuarioId == userId && s.Activa)
                .FirstOrDefaultAsync();

            // Permitir lectura si tiene suscripción O si es acceso básico (primeros 3 libros)
            var librosLeidosHoy = await _context.HistorialLibros
                .Where(h => h.UsuarioId == userId && h.FechaLectura.Date == DateTime.Today)
                .CountAsync();

            if (suscripcion == null && librosLeidosHoy >= 3)
            {
                return Unauthorized("Has alcanzado tu límite diario de lectura. Suscríbete para acceso ilimitado.");
            }

            var libro = await _gutenberg.ObtenerLibro(bookId);
            if (libro == null)
                return NotFound();

            // Guardar en el historial
            var historial = new HistorialLibro
            {
                UsuarioId = userId,
                BookId = bookId,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                Imagen = libro.Imagen,
                FechaLectura = DateTime.UtcNow,
                LinkLectura = libro.LinkLectura
            };

            _context.HistorialLibros.Add(historial);
            await _context.SaveChangesAsync();

            return Ok(libro);
        }
    }
}