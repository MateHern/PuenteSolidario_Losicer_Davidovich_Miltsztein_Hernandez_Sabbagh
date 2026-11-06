using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProgramacionXEfsi_Puente.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RequireRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var rol = session.GetString("Rol");

            if (string.IsNullOrWhiteSpace(rol))
            {
                RedirectToLogin(context, "Debes iniciar sesión para acceder a esta vista.");
                return;
            }

            if (!_allowedRoles.Any(role => string.Equals(role, rol, StringComparison.OrdinalIgnoreCase)))
            {
                RedirectToLogin(context, $"Esta acción requiere uno de estos roles: {string.Join(", ", _allowedRoles)}.");
                return;
            }

            if (string.Equals(rol, "Usuario", StringComparison.OrdinalIgnoreCase) && !session.GetInt32("UsuarioId").HasValue)
            {
                RedirectToLogin(context, "Tu sesión de usuario no es válida.");
                return;
            }

            if (string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase) && !session.GetInt32("UsuarioId").HasValue)
            {
                RedirectToLogin(context, "Tu sesión de administrador no es válida.");
                return;
            }

            if (string.Equals(rol, "Ong", StringComparison.OrdinalIgnoreCase) && !session.GetInt32("OngId").HasValue)
            {
                RedirectToLogin(context, "Tu sesión de ONG no es válida.");
                return;
            }
        }

        private static void RedirectToLogin(AuthorizationFilterContext context, string message)
        {
            context.HttpContext.Session.Clear();
            context.HttpContext.Session.SetString("LoginError", message);
            context.Result = new RedirectToActionResult("Login", "Cuenta", null);
        }
    }
}
