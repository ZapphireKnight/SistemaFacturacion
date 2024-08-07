using System;
using System.Collections.Generic;

namespace Modulo_Facturacion.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}
