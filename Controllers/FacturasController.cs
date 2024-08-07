using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Modulo_Facturacion.Models;

namespace Modulo_Facturacion.Controllers
{
    public class FacturasController : Controller
    {
        private readonly SistemaFacturacionContext _context;

        public FacturasController(SistemaFacturacionContext context)
        {
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            var sistemaFacturacionContext = _context.Facturas.Include(f => f.IdArticuloNavigation).Include(f => f.IdClienteNavigation).Include(f => f.IdVendedorNavigation);
            return View(await sistemaFacturacionContext.ToListAsync());
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdArticuloNavigation)
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdVendedorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Facturas/Create
        public IActionResult Create()
        {
            var precioArticulo = _context.Articulos.Select(a => new { a.Id, a.PrecioUnitario }).ToList();

            ViewData["IdArticulo"] = new SelectList(_context.Articulos, "Id", "Descripcion");
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["IdVendedor"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PreciosUnitarios"] = precioArticulo.Select(a => a.PrecioUnitario).ToArray();

            return View();
        }

        // POST: Facturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdVendedor,IdCliente,Fecha,Comentario,IdArticulo,Cantidad,PrecioUnitario")] Factura factura)
        {
            if (ModelState.IsValid)
            {
                _context.Add(factura);
                await _context.SaveChangesAsync();


                var cliente = await _context.Clientes.FindAsync(factura.IdCliente);

                if (cliente != null)
                {
                    var montoAsiento = factura.Cantidad * factura.PrecioUnitario;
                    var tipoMovimiento = "";

                    if (cliente.CuentaContable == "13")
                    {
                        tipoMovimiento = "DB";
                    }
                    else{
                        tipoMovimiento = "CR";
                    }
                    

                    var asiento = new Asiento
                    {
                        FechaAsiento = DateOnly.FromDateTime(factura.Fecha),
                        IdentificadorCuenta = int.Parse(cliente.CuentaContable),
                        TipoMovimiento = tipoMovimiento,
                        MontoAsiento = montoAsiento
                    };

                    // Agregar el asiento al contexto
                    _context.Asientos.Add(asiento);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Registrar los errores de ModelState
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    // Aquí puedes usar una herramienta de logging, como NLog o Serilog
                    Console.WriteLine(error.ErrorMessage); // o registra el error de otra manera
                }
            }

            var precioArticulo = _context.Articulos.Select(a => new { a.Id, a.PrecioUnitario }).ToList();

            ViewData["IdArticulo"] = new SelectList(_context.Articulos, "Id", "Descripcion", factura.IdArticulo);
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Nombre", factura.IdCliente);
            ViewData["IdVendedor"] = new SelectList(_context.Empleados, "Id", "Nombre", factura.IdVendedor);
            ViewData["PreciosUnitarios"] = precioArticulo.Select(a => a.PrecioUnitario).ToArray();
            return View(factura);
        }

        // GET: Facturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }
            var precioArticulo = _context.Articulos.Select(a => new { a.Id, a.PrecioUnitario }).ToList();

            ViewData["IdArticulo"] = new SelectList(_context.Articulos, "Id", "Descripcion", factura.IdArticulo);
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Nombre", factura.IdCliente);
            ViewData["IdVendedor"] = new SelectList(_context.Empleados, "Id", "Nombre", factura.IdVendedor);
            ViewData["PreciosUnitarios"] = precioArticulo.Select(a => a.PrecioUnitario).ToArray();
            return View(factura);
        }

        // POST: Facturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdVendedor,IdCliente,Fecha,Comentario,IdArticulo,Cantidad,PrecioUnitario")] Factura factura)
        {
            if (id != factura.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factura);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaExists(factura.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var precioArticulo = _context.Articulos.Select(a => new { a.Id, a.PrecioUnitario }).ToList();

            ViewData["IdArticulo"] = new SelectList(_context.Articulos, "Id", "Descripcion", factura.IdArticulo);
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Nombre", factura.IdCliente);
            ViewData["IdVendedor"] = new SelectList(_context.Empleados, "Id", "Nombre", factura.IdVendedor);
            ViewData["PreciosUnitarios"] = precioArticulo.Select(a => a.PrecioUnitario).ToArray();
            return View(factura);
        }

        // GET: Facturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdArticuloNavigation)
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdVendedorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                _context.Facturas.Remove(factura);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.Id == id);
        }
    }
}
