using Microsoft.AspNetCore.Mvc;

namespace ProgramacionXEfsi_Puente.Controllers;

public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        TempData["IsLoggedIn"] = true;
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        TempData["IsLoggedIn"] = false;
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult RegistroPersonal()
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegistroOrg()
    {
        return View();
    }
}
