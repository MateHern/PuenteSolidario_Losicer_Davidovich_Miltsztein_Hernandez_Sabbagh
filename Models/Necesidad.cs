namespace ProgramacionXEfsi_Puente.Models
{
    public class Necesidad
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }
        public int CategoriaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
