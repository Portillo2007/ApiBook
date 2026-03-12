using BibliotecaAPI.Data;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers
{
    [Authorize]
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

        [Authorize]
[HttpGet("read/{bookId}")]
public async Task<IActionResult> LeerLibro(int bookId)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var suscripcion = await _context.Suscripciones
        .Where(s => s.UsuarioId == int.Parse(userId) && s.Activa)
        .FirstOrDefaultAsync();

    if (suscripcion == null)
    {
        return Unauthorized("Necesitas una suscripción activa para leer libros");
    }

    var libro = await _gutenberg.ObtenerLibro(bookId);

    if (libro == null)
        return NotFound();

    return Ok(libro);
}
}
}