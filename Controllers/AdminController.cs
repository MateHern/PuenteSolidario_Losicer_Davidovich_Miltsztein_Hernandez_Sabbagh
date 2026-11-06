using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class AdminController : Controller
    {
        [RequireRole("Admin")]
        public IActionResult AprobarOngs()
        {
            var pendientes = BD.ObtenerOngsPendientes();
            return View(pendientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin")]
        public IActionResult AprobarOngs(int id)
        {
            BD.AprobarOrganizacion(id);
            return RedirectToAction(nameof(AprobarOngs));
        }

        [HttpGet]
        [RequireRole("Admin")]
        public IActionResult CrearAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin")]
        public IActionResult CrearAdmin(string nombre, string apellido, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Complete todos los campos.");
                return View();
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                return View();
            }

            BD.RegistrarAdmin(nombre.Trim(), apellido.Trim(), email.Trim(), password);
            TempData["AdminCreado"] = "Administrador creado correctamente.";
            return RedirectToAction(nameof(AprobarOngs));
        }
    }
}
