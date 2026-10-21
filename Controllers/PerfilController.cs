using Microsoft.AspNetCore.Mvc;

namespace ProgramacionXEfsi_Puente.Controllers;

public class PerfilController : Controller
{
    public IActionResult Index()
    {
        if (TempData["IsLoggedIn"] as bool? != true)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }
}
