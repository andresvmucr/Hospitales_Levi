using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Doctore
{
    public string Cedula { get; set; } = null!;

    public string IdEspecialidad { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual Usuario CorreoNavigation { get; set; } = null!;

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual Especialidade IdEspecialidadNavigation { get; set; } = null!;
}
