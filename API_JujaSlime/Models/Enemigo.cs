using System.Collections.Generic;

namespace CyberWiki.API.Models
{
    public class Enemigo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Vida { get; set; }
        public string ImagePath { get; set; }
        public List<HabilidadEnemigo> Abilities { get; set; } = new List<HabilidadEnemigo>();
    }

    public class HabilidadEnemigo
    {
        public string Nombre { get; set; }
        public string DescripcionDetallada { get; set; }
    }
}