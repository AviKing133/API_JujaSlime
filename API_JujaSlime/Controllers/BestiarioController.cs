using Microsoft.AspNetCore.Mvc;
using CyberWiki.API.Models;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace CyberWiki.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestiarioController : ControllerBase
    {
        private readonly string _connectionString;

        public BestiarioController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 1. OBTENER TODO EL BESTIARIO
        // Útil para cargar la lista de nombres en el WPF al principio
        [HttpGet]
        public async Task<IActionResult> GetBestiario()
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            string sql = @"
                SELECT e.Id, e.Nombre, e.Descripcion, e.Vida, e.ImagePath, 
                       h.Nombre, h.DescripcionDetallada 
                FROM Enemigos e
                LEFT JOIN HabilidadesEnemigos h ON e.Id = h.EnemigoId";

            var enemigoDict = new Dictionary<int, Enemigo>();

            await db.QueryAsync<Enemigo, HabilidadEnemigo, Enemigo>(
                sql,
                (enemigo, habilidad) =>
                {
                    if (!enemigoDict.TryGetValue(enemigo.Id, out var enemigoExistente))
                    {
                        enemigoExistente = enemigo;
                        enemigoExistente.Abilities = new List<HabilidadEnemigo>();
                        enemigoDict.Add(enemigoExistente.Id, enemigoExistente);
                    }

                    if (habilidad != null)
                    {
                        enemigoExistente.Abilities.Add(habilidad);
                    }

                    return enemigoExistente;
                },
                null,
                splitOn: "Nombre"
            );

            return Ok(enemigoDict.Values);
        }

        // 2. OBTENER UN ENEMIGO ESPECÍFICO POR ID
        // Útil para cuando el usuario hace clic en un nombre específico en el WPF
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnemigoById(int id)
        {
            using IDbConnection db = new MySqlConnection(_connectionString);

            string sql = @"
                SELECT e.Id, e.Nombre, e.Descripcion, e.Vida, e.ImagePath, 
                       h.Nombre, h.DescripcionDetallada 
                FROM Enemigos e
                LEFT JOIN HabilidadesEnemigos h ON e.Id = h.EnemigoId
                WHERE e.Id = @Id";

            var enemigoDict = new Dictionary<int, Enemigo>();

            await db.QueryAsync<Enemigo, HabilidadEnemigo, Enemigo>(
                sql,
                (enemigo, habilidad) =>
                {
                    if (!enemigoDict.TryGetValue(enemigo.Id, out var eExistente))
                    {
                        eExistente = enemigo;
                        eExistente.Abilities = new List<HabilidadEnemigo>();
                        enemigoDict.Add(eExistente.Id, eExistente);
                    }

                    if (habilidad != null)
                    {
                        eExistente.Abilities.Add(habilidad);
                    }

                    return eExistente;
                },
                new { Id = id }, // Pasamos el ID que recibimos por la URL
                splitOn: "Nombre"
            );

            var resultado = enemigoDict.Values.FirstOrDefault();

            if (resultado == null)
            {
                return NotFound(new { mensaje = "Enemigo no encontrado" });
            }

            return Ok(resultado);
        }
    }
}