using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Usuario
{
    public string Correo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string PrimerApellido { get; set; } = null!;

    public string SegundoApellido { get; set; } = null!;

    public string Contrasenna { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string IdHospital { get; set; } = null!;

    public virtual ICollection<Doctore> Doctores { get; set; } = new List<Doctore>();

    public virtual Hospitale IdHospitalNavigation { get; set; } = null!;

    public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
}
