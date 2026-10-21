using Microsoft.AspNetCore.Mvc;

namespace ProgramacionXEfsi_Puente.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
