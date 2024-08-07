using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modulo_Facturacion.Models;

namespace Modulo_Facturacion.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly SistemaFacturacionContext _context;

        public ReportesController(SistemaFacturacionContext context)
        {
            _context = context;
        }

        // GET: HomeController1
        public ActionResult Index(String reportName)
        {
            if (reportName == "articulos") {
                var topArticulos = _context.Facturas
                .GroupBy(f => f.IdArticulo)
                .Select(g => new ArticuloViewModel
                {
                    ArticuloId = g.Key,
                    Count = g.Count(),
                    Descripcion = g.FirstOrDefault().IdArticuloNavigation.Descripcion
                })
                .OrderByDescending(a => a.Count)
                .ToList();

                ViewBag.TopArticulos = topArticulos;
            }

            if (reportName == "empleados")
            {
                var topEmpleados = _context.Facturas
                .GroupBy(f => f.IdVendedor)
                .Select(g => new EmpleadoViewModel
                {
                    EmpleadoId = g.Key,
                    Count = g.Count(),
                    Empleado = g.FirstOrDefault().IdVendedorNavigation.Nombre
                })
                .OrderByDescending(e => e.Count)
                .ToList();

                ViewBag.TopEmpleados = topEmpleados;
            }

            if (reportName == "clientes")
            {
                var topClientes = _context.Facturas
               .GroupBy(f => f.IdCliente)
               .Select(g => new ClienteViewModel
               {
                   ClienteId = g.Key,
                   Count = g.Count(),
                   Cliente = g.FirstOrDefault().IdClienteNavigation.Nombre
               })
               .OrderByDescending(c => c.Count)
               .ToList();

                ViewBag.TopClientes = topClientes;
            }

            if (reportName == "facturas")
            {
                var facturasOrdenadas = _context.Facturas.Include(f => f.IdArticuloNavigation).Include(f => f.IdClienteNavigation).Include(f => f.IdVendedorNavigation)
                .OrderBy(f => f.Fecha)
                .ToList();

                ViewBag.FacturasOrdenadas = facturasOrdenadas;
            }

            return View();
        }
    }
}
