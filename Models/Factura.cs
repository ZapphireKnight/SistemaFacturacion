using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace Modulo_Facturacion.Models;

public partial class Factura
{
    public int Id { get; set; }

    public int IdVendedor { get; set; }

    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; }

    public string? Comentario { get; set; }

    public int IdArticulo { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    [ValidateNever]
    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    [ValidateNever]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ValidateNever]
    public virtual Empleado IdVendedorNavigation { get; set; } = null!;
}
