namespace CyberWiki.API.Models
{
        public class HabilidadSujeto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string IconoPath { get; set; }
        public bool Desbloqueada { get; set; }
    }
}