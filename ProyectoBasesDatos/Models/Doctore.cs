using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoBasesDatos.Models;

public partial class Doctore
{
    [Key]
    [Required(ErrorMessage = "La cédula es requerida")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "La especialidad es requerida")]
    public string Especialidad { get; set; } = null!;

    [Required(ErrorMessage = "El correo es requerido")]
    public string Correo { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual Usuario CorreoNavigation { get; set; } = null!;

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();
}
