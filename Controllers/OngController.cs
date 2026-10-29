using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class OngController : Controller
    {
        [RequireRole("Ong")]
        public IActionResult Dashboard()
        {
            var ongId = HttpContext.Session.GetInt32("OngId");
            var publicaciones = ongId.HasValue ? BD.ObtenerPublicacionesPorOrganizacion(ongId.Value) : new List<Publicacion>();
            ViewBag.OngNombre = HttpContext.Session.GetString("OngNombre") ?? "ONG";
            return View(publicaciones);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Ong")]
        public IActionResult CrearPublicacion(string nombreDelPedido, string tipoDePropuesta, string descripcion, string ubicacion, string cosasQueNecesitan)
        {
            var ongId = HttpContext.Session.GetInt32("OngId");

            if (!ongId.HasValue)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            if (string.IsNullOrWhiteSpace(nombreDelPedido) || string.IsNullOrWhiteSpace(tipoDePropuesta) || string.IsNullOrWhiteSpace(descripcion))
            {
                TempData["ErrorPublicacion"] = "Completá nombre, tipo y descripción de la publicación.";
                return RedirectToAction("Dashboard");
            }

            BD.RegistrarPublicacion(ongId.Value, nombreDelPedido.Trim(), tipoDePropuesta.Trim(), descripcion.Trim(), ubicacion?.Trim() ?? string.Empty, cosasQueNecesitan?.Trim() ?? string.Empty);
            TempData["OkPublicacion"] = "Publicación creada correctamente.";
            return RedirectToAction("Dashboard");
        }
    }
}
