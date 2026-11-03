using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class UsuarioController : Controller
    {
        [RequireRole("Usuario")]
        public IActionResult Perfil()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var usuario = BD.ObtenerUsuarioPorId(usuarioId.Value);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            return View(usuario);
        }
    }
}
