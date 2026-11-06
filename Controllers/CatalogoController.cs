using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index(string? categoria, string? ubicacion, string? busqueda)
    {
        var organizaciones = BD.ObtenerOrganizacionesFiltradas(categoria, ubicacion);

        ViewBag.Categorias = BD.ObtenerCategoriasNecesidad();
        ViewBag.Provincias = BD.ObtenerProvincias();
        ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;
        ViewBag.UbicacionSeleccionada = ubicacion ?? string.Empty;
        ViewBag.Busqueda = busqueda ?? string.Empty;

        return View(organizaciones);
    }
}
