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

            if (model.TipoCuenta == "Usuario")
            {
                using var connection = new SqlConnection(ConnectionString);
                var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLoginRow>(
                    "SELECT TOP 1 Id, Nombre, Contraseña AS Clave, Rol FROM Usuarios WHERE Mail = @Email",
                    new { Email = email });

                if (usuario != null)
                {
                    string storedPassword = usuario.Clave ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(storedPassword) && string.Equals(model.Password, storedPassword, StringComparison.Ordinal))
                    {
                        if (!string.IsNullOrWhiteSpace(usuario.Rol) &&
                            (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(usuario.Rol, "AdminMadre", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(usuario.Rol, "Usuario", StringComparison.OrdinalIgnoreCase)))
                        {
                            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                            HttpContext.Session.SetString("Rol", usuario.Rol ?? "Usuario");
                            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Usuario");
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            else if (model.TipoCuenta == "Ong")
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

                    string storedPassword = ong.Clave ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(storedPassword) && string.Equals(model.Password, storedPassword, StringComparison.Ordinal))
                    {
                        HttpContext.Session.SetInt32("OngId", ong.Id);
                        HttpContext.Session.SetString("Rol", "Ong");
                        HttpContext.Session.SetString("OngNombre", ong.Nombre ?? "ONG");
                        return RedirectToAction("Index", "Home");
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
