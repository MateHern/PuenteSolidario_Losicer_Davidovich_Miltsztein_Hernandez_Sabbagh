namespace ProgramacionXEfsi_Puente.Models
{
    public class DonacionRecibida
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string NombreDonante { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string RedSocial { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
    }
}
