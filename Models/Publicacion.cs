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
        public string CosasQueNecesitan { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaLimite { get; set; }
        public string NombreOrganizacion { get; set; } = string.Empty;
        public string TelefonoOrganizacion { get; set; } = string.Empty;
        public string DireccionOrganizacion { get; set; } = string.Empty;
        public string BarrioOrganizacion { get; set; } = string.Empty;
    }
}
