using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class DonarController : Controller
{
    [RequireRole("Usuario")]
    public IActionResult Index(int? idOrganizacion, int? idPublicacion)
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

        Publicacion? publicacion = null;
        Organizacion? organizacionSeleccionada = null;

        if (idPublicacion.HasValue)
        {
            publicacion = BD.ObtenerPublicacionPorId(idPublicacion.Value);
            if (publicacion != null)
            {
                organizacionSeleccionada = BD.ObtenerOrganizacionPorId(publicacion.IdOrganizacion);
            }
        }

        if (organizacionSeleccionada == null && idOrganizacion.HasValue)
        {
            organizacionSeleccionada = BD.ObtenerOrganizacionPorId(idOrganizacion.Value);
        }

        organizacionSeleccionada ??= BD.ObtenerOrganizaciones().FirstOrDefault();

        if (organizacionSeleccionada == null)
        {
            TempData["DonacionError"] = "No hay organizaciones disponibles para recibir la donación.";
            return RedirectToAction("Index", "Catalogo");
        }

        ViewBag.UsuarioNombre = usuario.Nombre;
        ViewBag.UsuarioApellido = usuario.Apellido;
        ViewBag.UsuarioEmail = usuario.Email;
        ViewBag.OrganizacionSeleccionada = organizacionSeleccionada;
        ViewBag.OrganizacionId = organizacionSeleccionada.Id;
        ViewBag.Publicacion = publicacion;
        ViewBag.IdPublicacion = idPublicacion ?? publicacion?.Id ?? 0;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Usuario")]
    public IActionResult Submit(int idOrganizacion, int idPublicacion, string tipoAporte, string cantidad, string mensaje, bool donarAnonimamente)
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
        if (!usuarioId.HasValue)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        if (idPublicacion > 0)
        {
            var publicacion = BD.ObtenerPublicacionPorId(idPublicacion);
            if (publicacion == null)
            {
                TempData["DonacionError"] = "La publicación ya no está disponible.";
                return RedirectToAction("Index", "Catalogo");
            }

            idOrganizacion = publicacion.IdOrganizacion;
        }

        if (idOrganizacion <= 0)
        {
            TempData["DonacionError"] = "No se pudo identificar la organización destinataria.";
            return RedirectToAction("Index", "Catalogo");
        }

        if (string.IsNullOrWhiteSpace(tipoAporte))
        {
            tipoAporte = "Donación";
        }

        if (string.IsNullOrWhiteSpace(cantidad))
        {
            TempData["DonacionError"] = "Ingresá la cantidad o el detalle del aporte.";
            return RedirectToAction("Index", "Donar", new { idOrganizacion, idPublicacion });
        }

        var usuario = BD.ObtenerUsuarioPorId(usuarioId.Value);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        BD.RegistrarDonacion(usuario.Id, idOrganizacion, tipoAporte, cantidad, mensaje ?? string.Empty);

        if (donarAnonimamente)
        {
            TempData["DonacionAnonimaExito"] = "Estás donando como anónimo.";
        }

        TempData["DonacionExito"] = "¡Gracias por tu donación!";
        return RedirectToAction("Gracias", "Donar");
    }

    [RequireRole("Usuario")]
    public IActionResult Gracias()
    {
        return View();
    }
}
