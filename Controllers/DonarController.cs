using Microsoft.AspNetCore.Mvc;

namespace ProgramacionXEfsi_Puente.Controllers;

public class DonarController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Submit()
    {
        return RedirectToAction("Index", "Perfil");
    }
}
