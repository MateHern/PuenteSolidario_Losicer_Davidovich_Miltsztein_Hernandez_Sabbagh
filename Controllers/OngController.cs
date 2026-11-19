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

            if (ongId.HasValue)
            {
                var organizacion = BD.ObtenerOrganizacionPorId(ongId.Value);
                if (organizacion != null)
                {
                    var partesUbicacion = new[] { organizacion.Direccion, organizacion.Barrio }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase);

                    ViewBag.UbicacionOrganizacion = string.Join(", ", partesUbicacion);
                }
            }

            ViewBag.OngNombre = HttpContext.Session.GetString("OngNombre") ?? "ONG";
            return View(publicaciones);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Ong")]
        public IActionResult CrearPublicacion(string nombreDelPedido, string tipoDePropuesta, string descripcion, string[] necesidades, Dictionary<string, int>? cantidades, string? cosasQueNecesitan, DateTime? fechaLimite, IFormFile? imagen)
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

            if (fechaLimite.HasValue && fechaLimite.Value.Date <= DateTime.Today)
            {
                TempData["ErrorPublicacion"] = "La fecha límite debe ser posterior al día de hoy.";
                return RedirectToAction("Dashboard");
            }

            string? imagenUrl = null;
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName);
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!extensionesPermitidas.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    TempData["ErrorPublicacion"] = "La imagen debe ser JPG, JPEG, PNG o WEBP.";
                    return RedirectToAction("Dashboard");
                }

                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "publicaciones");
                Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                imagenUrl = $"/uploads/publicaciones/{nombreArchivo}";
            }

            var necesidadesTexto = necesidades != null && necesidades.Length > 0
                ? string.Join(", ", necesidades.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
                : string.Empty;

            var necesidadesCantidad = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (necesidades != null)
            {
                foreach (var necesidad in necesidades.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (cantidades != null && cantidades.TryGetValue(necesidad, out var cantidad) && cantidad > 0)
                    {
                        necesidadesCantidad[necesidad] = cantidad;
                    }
                }
            }

            if (necesidades != null && necesidades.Length > 0 && necesidadesCantidad.Count == 0)
            {
                TempData["ErrorPublicacion"] = "Indicá una cantidad mayor a cero para cada necesidad seleccionada.";
                return RedirectToAction("Dashboard");
            }

            try
            {
                var publicacionId = BD.RegistrarPublicacion(ongId.Value, nombreDelPedido.Trim(), tipoDePropuesta.Trim(), descripcion.Trim(), necesidadesTexto, cosasQueNecesitan?.Trim() ?? string.Empty, fechaLimite, imagenUrl);
                if (publicacionId > 0)
                {
                    if (cantidades != null)
                    {
                        BD.RegistrarNecesidadesPublicacion(publicacionId, necesidadesCantidad);
                    }

                    TempData["OkPublicacion"] = "Publicación creada correctamente.";
                    return RedirectToAction("Index", "Catalogo");
                }
                else
                {
                    TempData["ErrorPublicacion"] = "No se pudo crear la publicación. Revisá los datos e intentá nuevamente.";
                    return RedirectToAction("Dashboard");
                }
            }
            catch
            {
                TempData["ErrorPublicacion"] = "No se pudo crear la publicación. Revisá los datos e intentá nuevamente.";
                return RedirectToAction("Dashboard");
            }
        }

        [RequireRole("Ong")]
        public IActionResult Perfil()
        {
            var ongId = HttpContext.Session.GetInt32("OngId");

            if (!ongId.HasValue)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var organizacion = BD.ObtenerOrganizacionPorId(ongId.Value);
            if (organizacion == null)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var donaciones = BD.ObtenerDonacionesRecibidasPorOrganizacion(ongId.Value);
            ViewBag.DonacionesRecibidas = donaciones;

            return View(organizacion);
        }
    }
}
