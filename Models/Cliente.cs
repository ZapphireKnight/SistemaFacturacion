﻿using System;
using System.Collections.Generic;

namespace Modulo_Facturacion.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Identificacion { get; set; } = null!;

    public string? CuentaContable { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
