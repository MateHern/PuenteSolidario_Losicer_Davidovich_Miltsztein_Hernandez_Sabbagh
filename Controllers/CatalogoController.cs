using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgramacionXEfsi_Puente.Models;

namespace ProgramacionXEfsi_Puente.Controllers;

[AllowAnonymous]
public class CatalogoController : Controller
{
    public IActionResult Index(string? categoria, string? ubicacion, string? busqueda)
    {
        try
        {
            var publicaciones = BD.ObtenerPublicacionesFiltradas(categoria, ubicacion, busqueda);
            ViewBag.Categorias = BD.ObtenerCategoriasNecesidad();
            ViewBag.Provincias = BD.ObtenerProvincias();
            ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;
            ViewBag.UbicacionSeleccionada = ubicacion ?? string.Empty;
            ViewBag.Busqueda = busqueda ?? string.Empty;
            return View(publicaciones);
        }
        catch
        {
            ViewBag.Categorias = new List<string>();
            ViewBag.Provincias = new List<string>();
            ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;
            ViewBag.UbicacionSeleccionada = ubicacion ?? string.Empty;
            ViewBag.Busqueda = busqueda ?? string.Empty;
            return View(new List<Publicacion>());
        }
    }

    public IActionResult Detalle(int id)
    {
        try
        {
            var publicacion = BD.ObtenerPublicacionPorId(id);
            if (publicacion == null)
            {
                return NotFound();
            }

            return View(publicacion);
        }
        catch
        {
            return NotFound();
        }
    }
}
