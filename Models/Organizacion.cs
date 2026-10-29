namespace ProgramacionXEfsi_Puente.Models
{
    public class Organizacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Barrio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool EsAprobado { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
    }
}
