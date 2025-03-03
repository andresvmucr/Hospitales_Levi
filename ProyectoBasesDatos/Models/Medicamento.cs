using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Medicamento
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string IdHospitalMedicamento { get; set; } = null!;

    public virtual HospitalMed IdHospitalMedicamentoNavigation { get; set; } = null!;

    public virtual ICollection<TratamientosMed> TratamientosMeds { get; set; } = new List<TratamientosMed>();
}
