using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using CyberWiki.API.Models;
using CyberWiki.API.DTOs;
using System.Data;

namespace CyberWiki.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabilidadesController : ControllerBase
    {
        private readonly string _connectionString;

        public HabilidadesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetHabilidadesUsuario(int usuarioId)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            // Corregido: 'c.Descripcion' en lugar de 'c.Description'
            string sql = @"
        SELECT c.Id, c.Nombre, c.Descripcion, c.IconoPath,
               IFNULL(uh.Desbloqueada, 0) AS Desbloqueada
        FROM CatalogoHabilidadesSujeto c
        LEFT JOIN UsuarioHabilidades uh ON c.Id = uh.HabilidadId AND uh.UsuarioId = @UsuarioId";

            var habilidades = await db.QueryAsync<HabilidadSujeto>(sql, new { UsuarioId = usuarioId });

            return Ok(habilidades);
        }

        [HttpPost("desbloquear")]
        public async Task<IActionResult> Desbloquear([FromBody] AsignarHabilidadRequest request)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            // CAMBIO CLAVE: Como la fila ya existe tras el registro, hacemos un UPDATE
            // Cambiamos el 0 por un 1 para el usuario y habilidad específicos
            string sql = @"UPDATE UsuarioHabilidades 
                           SET Desbloqueada = 1 
                           WHERE UsuarioId = @UsuarioId AND HabilidadId = @HabilidadId";

            try
            {
                int filasAfectadas = await db.ExecuteAsync(sql, request);

                if (filasAfectadas > 0)
                {
                    return Ok(new { mensaje = "Habilidad activada correctamente en la base de datos" });
                }
                else
                {
                    // Si no afectó a ninguna fila, es que no existía la relación previa
                    return NotFound(new { mensaje = "No se encontró la habilidad asignada para este usuario" });
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { mensaje = "Error al activar la habilidad: " + ex.Message });
            }
        }
    }
}