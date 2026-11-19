using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class CuentaController : Controller
    {
        private static string NormalizarRol(string? rol)
        {
            return (rol ?? string.Empty).Trim();
        }

        private static string CanonicalizarRol(string? rol)
        {
            var normalized = NormalizarRol(rol);

            if (string.Equals(normalized, "AdminMadre", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            if (string.Equals(normalized, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Administrador", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            if (string.Equals(normalized, "Ong", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "ONG", StringComparison.OrdinalIgnoreCase))
            {
                return "Ong";
            }

            return "Usuario";
        }

        private static bool EsRolUsuarioValido(string? rol)
        {
            var normalized = CanonicalizarRol(rol);
            return string.Equals(normalized, "Usuario", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.LoginError = HttpContext.Session.GetString("LoginError");
            HttpContext.Session.Remove("LoginError");
            return View(new LoginInput { TipoCuenta = "Usuario" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginInput model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var email = model.Email.Trim();

                if (string.Equals(model.TipoCuenta, "Usuario", StringComparison.OrdinalIgnoreCase))
                {
                    var usuario = BD.ValidarLoginUsuario(email, model.Password);

                    if (usuario != null)
                    {
                        var rolDb = NormalizarRol(usuario.Rol);
                        var rolSesion = CanonicalizarRol(rolDb);

                        if (!EsRolUsuarioValido(rolDb))
                        {
                            ModelState.AddModelError(string.Empty, "Ese usuario no tiene un rol válido para iniciar sesión.");
                            return View(model);
                        }

                        HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                        HttpContext.Session.SetString("Rol", rolSesion);
                        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Usuario");

                        if (string.Equals(rolSesion, "Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("AprobarOngs", "Admin");
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (string.Equals(model.TipoCuenta, "Ong", StringComparison.OrdinalIgnoreCase))
                {
                    var ong = BD.ValidarLoginOrganizacion(email, model.Password);

                    if (ong != null)
                    {
                        if (ong.EsAprobado == false)
                        {
                            ModelState.AddModelError(string.Empty, "Su cuenta de ONG está pendiente de aprobación por un administrador.");
                            return View(model);
                        }

                        HttpContext.Session.SetInt32("OngId", ong.Id);
                        HttpContext.Session.SetString("Rol", "Ong");
                        HttpContext.Session.SetString("OngNombre", ong.Nombre ?? "ONG");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (string.Equals(model.TipoCuenta, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var usuario = BD.ValidarLoginUsuario(email, model.Password);

                    if (usuario != null)
                    {
                        var rolDb = NormalizarRol(usuario.Rol);
                        var rolSesion = CanonicalizarRol(rolDb);

                        if (!string.Equals(rolSesion, "Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            ModelState.AddModelError(string.Empty, "Ese usuario no tiene permisos de administrador.");
                            return View(model);
                        }

                        HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                        HttpContext.Session.SetString("Rol", rolSesion);
                        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Admin");
                        return RedirectToAction("AprobarOngs", "Admin");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, "Email, contraseña o tipo de cuenta incorrectos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email, contraseña o tipo de cuenta incorrectos.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
