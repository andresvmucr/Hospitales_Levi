using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Pago
{
    public string Id { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public decimal Total { get; set; }

    public string MetodoPago { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string CedulaPaciente { get; set; } = null!;

    public string IdCita { get; set; } = null!;

    public virtual Paciente CedulaPacienteNavigation { get; set; } = null!;

    public virtual Cita IdCitaNavigation { get; set; } = null!;
}
