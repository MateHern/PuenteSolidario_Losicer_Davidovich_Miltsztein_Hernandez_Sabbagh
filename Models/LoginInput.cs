using System.ComponentModel.DataAnnotations;

namespace ProgramacionXEfsi_Puente.Models
{
    public class LoginInput
    {
        [Required(ErrorMessage = "Debes seleccionar el tipo de cuenta")]
        [RegularExpression("^(Usuario|Ong)$", ErrorMessage = "Debes seleccionar Usuario u ONG")]
        public string TipoCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ingresar un email válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
