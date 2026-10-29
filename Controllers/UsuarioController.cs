using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class UsuarioController : Controller
    {
        [RequireRole("Usuario")]
        public IActionResult Perfil()
        {
            var usuario = new Usuario
            {
                Nombre = "Martina",
                Apellido = "López",
                Email = "martina.lopez@puentesolidario.org"
            };

            return View(usuario);
        }
    }
}
