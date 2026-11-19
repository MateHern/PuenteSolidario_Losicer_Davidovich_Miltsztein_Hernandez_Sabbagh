using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers
{
    public class UsuarioController : Controller
    {
        [RequireRole("Usuario")]
        public IActionResult Perfil()
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

            return View(usuario);
        }

        [HttpGet]
        [RequireRole("Usuario")]
        public IActionResult Editar()
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

            return View(new EditarPerfilInput
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FotoPerfil = usuario.FotoPerfil
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Usuario")]
        public IActionResult Editar(EditarPerfilInput model, IFormFile? foto)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            model.Id = usuarioId.Value;
            model.NombreUsuario = model.NombreUsuario?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Telefono = model.Telefono?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.NombreUsuario))
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "El correo electrónico es obligatorio.");
            }

            if (!string.IsNullOrEmpty(model.NuevaClave) || !string.IsNullOrEmpty(model.ConfirmarClave))
            {
                if (model.NuevaClave != model.ConfirmarClave)
                {
                    ModelState.AddModelError("ConfirmarClave", "Las contraseñas no coinciden.");
                }
                else if (model.NuevaClave!.Length < 4)
                {
                    ModelState.AddModelError("NuevaClave", "La contraseña debe tener al menos 4 caracteres.");
                }
            }

            if (foto != null && foto.Length > 0)
            {
                var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "La foto debe ser JPG, JPEG, PNG o WEBP.");
                }
                else if (foto.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "La foto no puede superar los 5 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                var actual = BD.ObtenerUsuarioPorId(usuarioId.Value);
                model.FotoPerfil = actual?.FotoPerfil ?? model.FotoPerfil;
                return View(model);
            }

            var usuarioActual = BD.ObtenerUsuarioPorId(usuarioId.Value);
            var fotoPerfil = usuarioActual?.FotoPerfil ?? string.Empty;

            if (foto != null && foto.Length > 0)
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perfiles");
                Directory.CreateDirectory(carpeta);
                var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                foto.CopyTo(stream);
                fotoPerfil = $"/uploads/perfiles/{nombreArchivo}";
            }

            try
            {
                if (BD.ActualizarPerfilUsuario(usuarioId.Value, model.NombreUsuario, model.Email,
                    model.Telefono, model.NuevaClave, fotoPerfil))
                {
                    HttpContext.Session.SetString("UsuarioNombre", usuarioActual?.Nombre ?? "Usuario");
                    TempData["OkPerfil"] = "Perfil actualizado correctamente.";
                    return RedirectToAction("Perfil");
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el perfil. El correo o nombre de usuario ya puede estar en uso.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios.");
            }

            model.FotoPerfil = fotoPerfil;
            return View(model);
        }
    }
}
