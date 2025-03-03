using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoBasesDatos.Models;

public partial class Usuario
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El primer apellido es obligatorio")]
    public string Primerapellido { get; set; } = null!;

    [Required(ErrorMessage = "El segundo apellido es obligatorio")]
    public string Segundoapellido { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Contrasenna { get; set; } = null!;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public string Rol { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El código del hospital es obligatorio")]
    public string IdHospital { get; set; } = null!;

    public virtual ICollection<Doctore> Doctores { get; set; } = new List<Doctore>();

    public virtual Hospitale IdHospitalNavigation { get; set; } = null!;

    public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
}
