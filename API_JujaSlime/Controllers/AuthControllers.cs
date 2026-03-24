using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using CyberWiki.API.Models;
using System.Data;
using BCrypt.Net;

namespace CyberWiki.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;

        public AuthController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            string sql = "SELECT Id, Username, NivelActual, PasswordHash FROM Usuarios WHERE Username = @Username";
            var userRecord = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = request.Username });

            if (userRecord == null) return Unauthorized(new { mensaje = "Usuario no encontrado" });

            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, userRecord.PasswordHash);

            if (!isValid) return Unauthorized(new { mensaje = "Contraseña incorrecta" });

            return Ok(new Usuario
            {
                Id = userRecord.Id,
                Username = userRecord.Username,
                NivelActual = userRecord.NivelActual
            });
        }

        [HttpPost("update-nivel")]
        public async Task<IActionResult> UpdateNivel([FromBody] UpdateNivelRequest request)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);
            string sql = "UPDATE Usuarios SET NivelActual = @NuevoNivel WHERE Username = @Username";

            try
            {
                int filasAfectadas = await db.ExecuteAsync(sql, new
                {
                    Username = request.Username,
                    NuevoNivel = request.NuevoNivel
                });

                if (filasAfectadas > 0) return Ok(new { mensaje = "Progreso guardado correctamente" });

                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar nivel: " + ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, salt);

            // 1. Insertamos el usuario y obtenemos su nuevo ID en una sola transacción
            string sqlInsertUser = @"INSERT INTO Usuarios (Username, PasswordHash, NivelActual) 
                                   VALUES (@Username, @PasswordHash, 1);
                                   SELECT LAST_INSERT_ID();";

            try
            {
                // Ejecutamos la inserción y recuperamos el ID generado
                int nuevoUsuarioId = await db.QuerySingleAsync<int>(sqlInsertUser, new
                {
                    Username = request.Username,
                    PasswordHash = passwordHash
                });

                // 2. Vinculamos TODAS las habilidades del catálogo al nuevo usuario
                // Por defecto aparecerán como no desbloqueadas en tu lógica de Unity
                string sqlAsignarHabilidades = @"
                    INSERT INTO UsuarioHabilidades (UsuarioId, HabilidadId)
                    SELECT @UsuarioId, Id FROM CatalogoHabilidadesSujeto";

                await db.ExecuteAsync(sqlAsignarHabilidades, new { UsuarioId = nuevoUsuarioId });

                return Ok(new { mensaje = "Usuario registrado y catálogo de habilidades inicializado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al registrar: " + ex.Message });
            }
        }
    }

    public class LoginRequest { public string Username { get; set; } public string Password { get; set; } }
    public class RegisterRequest { public string Username { get; set; } public string Password { get; set; } }
    public class UpdateNivelRequest { public string Username { get; set; } public int NuevoNivel { get; set; } }
}