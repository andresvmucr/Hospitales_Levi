using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class HospitalMed
{
    public string Id { get; set; } = null!;

    public int Precio { get; set; }

    public int Cantidad { get; set; }

    public string IdHospital { get; set; } = null!;

    public virtual Hospitale IdHospitalNavigation { get; set; } = null!;

    public virtual ICollection<Medicamento> Medicamentos { get; set; } = new List<Medicamento>();
}
