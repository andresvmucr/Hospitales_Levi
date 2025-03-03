using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Horario
{
    public string Dia { get; set; } = null!;

    public DateTime Horainicio { get; set; }

    public DateTime Horafin { get; set; }

    public string CedulaDoctor { get; set; } = null!;

    public virtual Doctore CedulaDoctorNavigation { get; set; } = null!;
}
