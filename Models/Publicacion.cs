namespace ProgramacionXEfsi_Puente.Models
{
    public class Publicacion
    {
        public int Id { get; set; }
        public int IdOrganizacion { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string TipoPropuesta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Necesidades { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
