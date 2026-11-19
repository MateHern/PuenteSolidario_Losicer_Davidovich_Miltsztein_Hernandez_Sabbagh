using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return RedirectToAction("Login", "Cuenta");
    }

    [HttpGet]
    public IActionResult RegistroPersonal()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegistroPersonal(string nombre, string apellido, string correo, string telefono, string redSocial, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Todos los campos son obligatorios.");
            return View();
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
            return View();
        }

        if (BD.ExisteUsuarioPorEmail(correo.Trim()))
        {
            ModelState.AddModelError(string.Empty, "Ya existe un usuario con ese correo.");
            return View();
        }

        BD.RegistrarUsuario(nombre.Trim(), apellido.Trim(), correo.Trim(), password, telefono?.Trim() ?? string.Empty, redSocial?.Trim() ?? string.Empty);
        TempData["RegistroExito"] = "Cuenta personal creada correctamente.";
        return RedirectToAction("Login", "Cuenta");
    }

    [HttpGet]
    public IActionResult RegistroOrg()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegistroOrg(string nombrePublico, string correoInstitucional, string password, string confirmPassword, string telefono, string descripcion, string ubicacionCalle, string ubicacionNumero, string ubicacionLocalidad, string[] necesidades)
    {
        if (string.IsNullOrWhiteSpace(nombrePublico) || string.IsNullOrWhiteSpace(correoInstitucional) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Completa nombre, correo y contraseña para registrar la organización.");
            return View();
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
            return View();
        }

        if (BD.ExisteOrganizacionPorEmail(correoInstitucional.Trim()))
        {
            ModelState.AddModelError(string.Empty, "Ya existe una organización con ese correo.");
            return View();
        }

        var idOrganizacion = BD.RegistrarOrganizacion(
            nombrePublico.Trim(),
            correoInstitucional.Trim(),
            password,
            telefono ?? string.Empty,
            descripcion ?? string.Empty,
            ubicacionCalle ?? string.Empty,
            ubicacionNumero ?? string.Empty,
            ubicacionLocalidad ?? string.Empty);

        BD.RegistrarNecesidadesOrganizacion(idOrganizacion, necesidades);

        TempData["RegistroExito"] = "Tu solicitud de ONG fue enviada y queda pendiente de aprobación por un administrador.";
        return RedirectToAction("Login", "Cuenta");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
