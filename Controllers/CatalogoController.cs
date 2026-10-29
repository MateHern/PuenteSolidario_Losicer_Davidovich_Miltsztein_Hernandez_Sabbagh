using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index()
    {
        var organizaciones = BD.ObtenerOrganizaciones();
        return View(organizaciones);
    }
}
