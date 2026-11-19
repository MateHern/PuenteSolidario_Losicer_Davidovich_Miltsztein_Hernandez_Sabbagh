using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var rol = HttpContext.Session.GetString("Rol");
        var isLoggedIn = !string.IsNullOrWhiteSpace(rol);

        ViewBag.IsLoggedIn = isLoggedIn;
        ViewBag.IsOng = string.Equals(rol, "Ong", StringComparison.OrdinalIgnoreCase);
        ViewBag.IsAdmin = string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase);

        if (isLoggedIn)
        {
            if (string.Equals(rol, "Usuario", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId.HasValue)
                {
                    var usuario = BD.ObtenerUsuarioPorId(usuarioId.Value);
                    ViewBag.UsuarioNombre = usuario?.Nombre ?? HttpContext.Session.GetString("UsuarioNombre") ?? "Usuario";
                    ViewBag.UsuarioEmail = usuario?.Email ?? "";
                }
                else
                {
                    ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre") ?? "Usuario";
                    ViewBag.UsuarioEmail = "";
                }
            }
            else if (string.Equals(rol, "Ong", StringComparison.OrdinalIgnoreCase))
            {
                var ongId = HttpContext.Session.GetInt32("OngId");
                if (ongId.HasValue)
                {
                    var ong = BD.ObtenerOrganizacionPorId(ongId.Value);
                    ViewBag.UsuarioNombre = ong?.Nombre ?? HttpContext.Session.GetString("OngNombre") ?? "ONG";
                    ViewBag.UsuarioEmail = "";
                }
                else
                {
                    ViewBag.UsuarioNombre = HttpContext.Session.GetString("OngNombre") ?? "ONG";
                    ViewBag.UsuarioEmail = "";
                }
            }
        }

        ViewBag.ComentariosLanding = BD.ObtenerComentariosLanding();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AgregarComentario(string texto)
    {
        var rol = HttpContext.Session.GetString("Rol") ?? string.Empty;
        var puedeComentar = string.Equals(rol, "Usuario", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, "Ong", StringComparison.OrdinalIgnoreCase);

        if (!puedeComentar)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            TempData["ComentarioError"] = "Escribí un comentario antes de publicar.";
            return RedirectToAction(nameof(Index));
        }

        var nombre = string.Equals(rol, "Ong", StringComparison.OrdinalIgnoreCase)
            ? HttpContext.Session.GetString("OngNombre") ?? "ONG"
            : HttpContext.Session.GetString("UsuarioNombre") ?? "Usuario";

        try
        {
            BD.AgregarComentarioLanding(nombre, rol, texto.Trim());
            TempData["ComentarioExito"] = "Tu comentario fue publicado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo publicar un comentario en la landing page.");
            TempData["ComentarioError"] = "No se pudo publicar el comentario. Intentá nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
