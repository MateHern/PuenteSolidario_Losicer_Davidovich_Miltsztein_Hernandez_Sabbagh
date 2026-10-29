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

        if (isLoggedIn)
        {
            if (string.Equals(rol, "Usuario", StringComparison.OrdinalIgnoreCase))
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

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
