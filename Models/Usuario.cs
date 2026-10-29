namespace ProgramacionXEfsi_Puente.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public bool EsAprobado { get; set; } = true;
    }
}
