using System;
using System.Collections.Generic;

namespace Modulo_Facturacion.Models;

public partial class Asiento
{
    public int Id { get; set; }

    public int? IdentificadorAsiento { get; set; }

    public DateOnly? FechaAsiento { get; set; }

    public int IdentificadorCuenta { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal MontoAsiento { get; set; }
}
