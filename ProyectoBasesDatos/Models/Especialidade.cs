using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Especialidade
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Doctore> Doctores { get; set; } = new List<Doctore>();
}
