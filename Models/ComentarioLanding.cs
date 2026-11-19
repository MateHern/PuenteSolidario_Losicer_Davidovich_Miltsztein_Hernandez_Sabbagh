namespace ProgramacionXEfsi_Puente.Models
{
    public class ComentarioLanding
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; }
    }
}
