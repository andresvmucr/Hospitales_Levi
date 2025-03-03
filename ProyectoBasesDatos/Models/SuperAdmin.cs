using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class SuperAdmin
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasenna { get; set; } = null!;

    public virtual ICollection<Hospitale> Hospitales { get; set; } = new List<Hospitale>();
}
