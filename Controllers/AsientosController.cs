using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Modulo_Facturacion.Models;

namespace Modulo_Facturacion.Controllers
{
    public class AsientosController : Controller
    {
        private readonly SistemaFacturacionContext _context;

        public AsientosController(SistemaFacturacionContext context)
        {
            _context = context;
        }

        // GET: Asientos
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var asientos = _context.Asientos.AsQueryable();

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                var fechaInicioDateOnly = DateOnly.FromDateTime(fechaInicio.Value);
                var fechaFinDateOnly = DateOnly.FromDateTime(fechaFin.Value);

                asientos = asientos.Where(a => a.FechaAsiento >= fechaInicioDateOnly && a.FechaAsiento <= fechaFinDateOnly && a.IdentificadorAsiento == null);

                ViewData["FechaInicio"] = fechaInicio.Value.ToString("yyyy-MM-dd");
                ViewData["FechaFin"] = fechaFin.Value.ToString("yyyy-MM-dd");

                return View(await asientos.ToListAsync());
            }

            return View();
        }

        // GET: Asientos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asiento = await _context.Asientos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asiento == null)
            {
                return NotFound();
            }

            return View(asiento);
        }

        // GET: Asientos/Create
        public async Task<IActionResult> Create(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio == null || fechaFin == null)
            {
                return BadRequest("Debe proporcionar fechas de inicio y fin.");
            }

            var asientos = await _context.Asientos
                .Where(a => a.FechaAsiento >= DateOnly.FromDateTime(fechaInicio.Value) && a.FechaAsiento <= DateOnly.FromDateTime(fechaFin.Value) && a.IdentificadorAsiento == null)
                .ToListAsync();

            var resumen = asientos
                .GroupBy(a => new { a.IdentificadorCuenta, a.TipoMovimiento })
                .Select(g => new
                {
                    Cuenta = g.Key.IdentificadorCuenta,
                    TipoMovimiento = g.Key.TipoMovimiento,
                    TotalMonto = g.Sum(a => a.MontoAsiento)
                })
                .ToList();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            var combinedResponses = new StringBuilder();

            // Lógica para enviar los datos al servicio SOAP
            try
            {
                foreach (var item in resumen)
                {
                    var (success, responseMessage, asientoId) = await EnviarAsientoContable(item.Cuenta, item.TipoMovimiento, item.TotalMonto, fechaFin.Value);
                    if (!success)
                    {
                        ViewBag.Message = $"Error al enviar los datos al servicio SOAP: {responseMessage}";
                        return View();
                    }
                    combinedResponses.AppendLine(responseMessage);

                    // Actualizar los asientos en la base de datos con el IdentificadorAsiento retornado
                    var asientosToUpdate = _context.Asientos.Where(a => a.IdentificadorCuenta == item.Cuenta && a.IdentificadorAsiento == null).ToList();
                    foreach (var asiento in asientosToUpdate)
                    {
                        asiento.IdentificadorAsiento = asientoId;
                    }
                    await _context.SaveChangesAsync();
                }

                ViewBag.Message = $"Datos enviados correctamente al servicio SOAP. Respuestas: {combinedResponses}";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View();
        }

        private async Task<(bool Success, string ResponseMessage, int AsientoId)> EnviarAsientoContable(int cuenta, string tipoMovimiento, decimal monto, DateTime fechaFin)
        {
            using (var client = new HttpClient())
            {
                XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace tempuri = "http://tempuri.org/";

                var descripcion = $"Descripción del asiento {fechaFin:yyyy/MM}";

                var soapEnvelope = new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soap", soapEnv),
                    new XElement(soapEnv + "Body",
                        new XElement(tempuri + "AsientoContable",
                            new XElement(tempuri + "idAuxiliarOrigen", 1), // Puedes cambiar este valor según tus necesidades
                            new XElement(tempuri + "descripcion", descripcion),
                            new XElement(tempuri + "cuentaDB", tipoMovimiento == "DB" ? cuenta : 0),
                            new XElement(tempuri + "cuentaCR", tipoMovimiento == "CR" ? cuenta : 0),
                            new XElement(tempuri + "monto", (double)monto)
                        )
                    )
                );

                var content = new StringContent(soapEnvelope.ToString(), Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tempuri.org/AsientoContable");

                var response = await client.PostAsync("http://www.contabilidadws.somee.com/SSWS.asmx", content);
                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var asientoId = ExtractAsientoId(responseMessage);
                    return (true, responseMessage, asientoId);
                }

                return (false, responseMessage, 0);
            }
        }

        private int ExtractAsientoId(string responseMessage)
        {
            XDocument doc = XDocument.Parse(responseMessage);
            XNamespace ns = "http://tempuri.org/";
            var asientoIdElement = doc.Descendants(ns + "AsientoContableResult").FirstOrDefault();
            return asientoIdElement != null ? int.Parse(asientoIdElement.Value) : 0;
        }

        // POST: Asientos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdentificadorAsiento,FechaAsiento,IdentificadorCuenta,TipoMovimiento,MontoAsiento")] Asiento asiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asiento);
        }

        // GET: Asientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asiento = await _context.Asientos.FindAsync(id);
            if (asiento == null)
            {
                return NotFound();
            }
            return View(asiento);
        }

        // POST: Asientos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdentificadorAsiento,FechaAsiento,IdentificadorCuenta,TipoMovimiento,MontoAsiento")] Asiento asiento)
        {
            if (id != asiento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsientoExists(asiento.Id))
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
            return View(asiento);
        }

        // GET: Asientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asiento = await _context.Asientos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asiento == null)
            {
                return NotFound();
            }

            return View(asiento);
        }

        // POST: Asientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asiento = await _context.Asientos.FindAsync(id);
            if (asiento != null)
            {
                _context.Asientos.Remove(asiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsientoExists(int id)
        {
            return _context.Asientos.Any(e => e.Id == id);
        }
    }
}
