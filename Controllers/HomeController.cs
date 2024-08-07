using Microsoft.AspNetCore.Mvc;
using Modulo_Facturacion.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Modulo_Facturacion.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SistemaFacturacionContext _context;

        public HomeController(ILogger<HomeController> logger, SistemaFacturacionContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clientesCount = await _context.Clientes.CountAsync();
            var articulosCount = await _context.Articulos.CountAsync();
            var facturasCount = await _context.Facturas.CountAsync();
            var allCustomers = await _context.Clientes.ToListAsync();
            var facturas = _context.Facturas.ToList();
            decimal ventasTotales = facturas.Sum(f => f.PrecioUnitario * f.Cantidad);

            ViewData["VentasTotales"] = ventasTotales;
            ViewData["clientesCount"] = clientesCount;
            ViewData["articulosCount"] = articulosCount;
            ViewData["facturasCount"] = facturasCount;
            ViewData["allCustomers"] = allCustomers;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Accounts");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
