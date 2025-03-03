using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Cita
{
    public string Id { get; set; } = null!;

    public DateOnly Dia { get; set; }

    public DateTime Hora { get; set; }

    public string Estado { get; set; } = null!;

    public string CedulaPaciente { get; set; } = null!;

    public string CedulaDoctor { get; set; } = null!;

    public virtual Doctore CedulaDoctorNavigation { get; set; } = null!;

    public virtual Paciente CedulaPacienteNavigation { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}
