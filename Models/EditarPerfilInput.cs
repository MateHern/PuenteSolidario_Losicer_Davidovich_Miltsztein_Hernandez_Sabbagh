namespace ProgramacionXEfsi_Puente.Models
{
    public class EditarPerfilInput
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? NuevaClave { get; set; }
        public string? ConfirmarClave { get; set; }
        public string FotoPerfil { get; set; } = string.Empty;
    }
}
