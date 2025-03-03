using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Paciente
{
    public string Cedula { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Genero { get; set; } = null!;

    public DateOnly Fechanacimiento { get; set; }

    public string Correo { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual Usuario CorreoNavigation { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
