using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class DonarController : Controller
{
    [RequireRole("Usuario")]
    public IActionResult Index()
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

        ViewBag.UsuarioNombre = usuario.Nombre;
        ViewBag.UsuarioApellido = usuario.Apellido;
        ViewBag.UsuarioEmail = usuario.Email;
        ViewBag.Organizaciones = BD.ObtenerOrganizaciones();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Usuario")]
    public IActionResult Submit(int idOrganizacion, string tipoAporte, string cantidad, string mensaje, bool donarAnonimamente)
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
        if (!usuarioId.HasValue)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        if (idOrganizacion <= 0)
        {
            idOrganizacion = BD.ObtenerOrganizaciones().FirstOrDefault()?.Id ?? 1;
        }

        if (string.IsNullOrWhiteSpace(tipoAporte))
        {
            tipoAporte = "Donación";
        }

        if (string.IsNullOrWhiteSpace(cantidad))
        {
            ModelState.AddModelError(string.Empty, "Ingresá la cantidad o detalle del aporte.");
            ViewBag.Organizaciones = BD.ObtenerOrganizaciones();
            return View("Index");
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
