using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest login)
        {
            var user = _context.Usuarios
                .FirstOrDefault(u => u.Email == login.correo && u.Password == login.password);

            if (user == null)
                return Unauthorized("Credenciales incorrectas");

            var token = GenerarToken(user);

            return Ok(new
            {
                token = token,
                id_usuario = user.Id,
                nombre = user.Nombre ?? "Usuario",
                correo = user.Email,
                role = user.Rol ?? "FREE"
            });
        }

        [HttpPost("register")]
        public IActionResult Register(LoginRequest registro)
        {
            var usuario = new Usuario
            {
                Nombre = registro.correo.Split('@')[0], // Extraer nombre del email
                Email = registro.correo,
                Password = registro.password,
                Rol = "FREE"
            };
            
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Ok(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Email,
                rol = usuario.Rol
            });
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("EstaEsUnaClaveSuperSeguraParaJWT2026BibliotecaAPI"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}