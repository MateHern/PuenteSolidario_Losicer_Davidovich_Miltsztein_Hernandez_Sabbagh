using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class CuentaController : Controller
    {
        private const string ConnectionString = @"Server=(local);Database=Puente Solidario;Trusted_Connection=True;TrustServerCertificate=True;";

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

        private class UsuarioLoginRow
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
        }

        private class OngLoginRow
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
            public bool EsAprobado { get; set; }
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
        public async Task<IActionResult> Login(LoginInput model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            if (string.Equals(model.TipoCuenta, "Usuario", StringComparison.OrdinalIgnoreCase))
            {
                using var connection = new SqlConnection(ConnectionString);
                var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLoginRow>(
                    "SELECT TOP 1 Id, Nombre, Contraseña AS Clave, Rol FROM Usuarios WHERE Mail = @Email",
                    new { Email = email });

                if (usuario != null)
                {
                    var storedPassword = usuario.Clave ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(storedPassword) && string.Equals(model.Password, storedPassword, StringComparison.Ordinal))
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
            }
            else if (string.Equals(model.TipoCuenta, "Ong", StringComparison.OrdinalIgnoreCase))
            {
                using var connection = new SqlConnection(ConnectionString);
                var ong = await connection.QueryFirstOrDefaultAsync<OngLoginRow>(
                    "SELECT TOP 1 Id, Nombre, Contraseña AS Clave, Valida AS EsAprobado FROM Organizaciones WHERE Mail = @Email",
                    new { Email = email });

                if (ong != null)
                {
                    if (ong.EsAprobado == false)
                    {
                        ModelState.AddModelError(string.Empty, "Su cuenta de ONG está pendiente de aprobación por un administrador.");
                        return View(model);
                    }

                    var storedPassword = ong.Clave ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(storedPassword) && string.Equals(model.Password, storedPassword, StringComparison.Ordinal))
                    {
                        HttpContext.Session.SetInt32("OngId", ong.Id);
                        HttpContext.Session.SetString("Rol", "Ong");
                        HttpContext.Session.SetString("OngNombre", ong.Nombre ?? "ONG");
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            else if (string.Equals(model.TipoCuenta, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                using var connection = new SqlConnection(ConnectionString);
                var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLoginRow>(
                    "SELECT TOP 1 Id, Nombre, Contraseña AS Clave, Rol FROM Usuarios WHERE Mail = @Email",
                    new { Email = email });

                if (usuario != null)
                {
                    var storedPassword = usuario.Clave ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(storedPassword) && string.Equals(model.Password, storedPassword, StringComparison.Ordinal))
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
