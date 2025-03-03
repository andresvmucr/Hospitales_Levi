using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Hospitale
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Idsuperadim { get; set; } = null!;

    public virtual ICollection<HospitalMed> HospitalMeds { get; set; } = new List<HospitalMed>();

    public virtual SuperAdmin IdsuperadimNavigation { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
