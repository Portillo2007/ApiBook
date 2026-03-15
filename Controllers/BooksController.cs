using BibliotecaAPI.Data;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BibliotecaAPI.Models;
using System.Linq;

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

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            // Devolver libros populares por defecto
            var books = await _gutenberg.SearchBooks("classic");
            return Ok(books.Take(10));
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
        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscription()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Usuario no autenticado");
            }

            var userId = int.Parse(userIdClaim);

            // Obtener suscripción activa del usuario
            var suscripcion = await _context.Suscripciones
                .Where(s => s.UsuarioId == userId && s.Activa)
                .FirstOrDefaultAsync();

            // Contar libros leídos hoy
            var librosLeidosHoy = await _context.HistorialLibros
                .Where(h => h.UsuarioId == userId && h.FechaLectura.Date == DateTime.Today)
                .CountAsync();

            if (suscripcion == null)
            {
                return Ok(new
                {
                    TieneSuscripcion = false,
                    TipoPlan = "Ninguno",
                    LimiteDiario = 3,
                    LeidosHoy = librosLeidosHoy,
                    RestantesHoy = Math.Max(0, 3 - librosLeidosHoy),
                    EsPremium = false
                });
            }

            var limiteDiario = suscripcion.LimiteLibrosDiario;
            var restantesHoy = Math.Max(0, limiteDiario - librosLeidosHoy);

            return Ok(new
            {
                TieneSuscripcion = true,
                TipoPlan = suscripcion.TipoPlan,
                Precio = suscripcion.Precio,
                FechaInicio = suscripcion.FechaInicio,
                FechaFin = suscripcion.FechaFin,
                LimiteDiario = limiteDiario,
                LeidosHoy = librosLeidosHoy,
                RestantesHoy = restantesHoy,
                EsPremium = suscripcion.EsPremium
            });
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

            // Obtener suscripción activa del usuario
            var suscripcion = await _context.Suscripciones
                .Where(s => s.UsuarioId == userId && s.Activa)
                .FirstOrDefaultAsync();

            // Verificar límite de lectura según el plan
            var limiteDiario = suscripcion?.LimiteLibrosDiario ?? 3; // Por defecto 3 para usuarios sin suscripción

            // Contar libros leídos hoy
            var librosLeidosHoy = await _context.HistorialLibros
                .Where(h => h.UsuarioId == userId && h.FechaLectura.Date == DateTime.Today)
                .CountAsync();

            // Verificar si puede leer más libros (solo usuarios básicos tienen límite)
            if (librosLeidosHoy >= limiteDiario && suscripcion?.EsPremium != true)
            {
                var mensaje = suscripcion?.EsPremium == true 
                    ? "Como usuario Premium, tienes acceso ilimitado. Contacta soporte si hay un error."
                    : $"Has alcanzado tu límite diario de {limiteDiario} libros. Suscríbete a Premium para acceso ilimitado.";
                return Unauthorized(mensaje);
            }

            var libro = await _gutenberg.ObtenerLibro(bookId);
            if (libro == null)
                return NotFound();

            // Todos los libros son accesibles, no hay distinción premium
            libro.Premium = false;

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