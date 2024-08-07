using System;
using System.Collections.Generic;

namespace Modulo_Facturacion.Models;

public partial class Articulo
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
