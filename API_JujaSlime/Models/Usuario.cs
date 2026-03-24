namespace CyberWiki.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int NivelActual { get; set; }
    }
}