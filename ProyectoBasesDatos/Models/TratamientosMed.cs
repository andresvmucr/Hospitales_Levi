using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class TratamientosMed
{
    public string Id { get; set; } = null!;

    public string Dosis { get; set; } = null!;

    public string Frecuencia { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public string IdTratamiento { get; set; } = null!;

    public string IdMedicamento { get; set; } = null!;

    public virtual Medicamento IdMedicamentoNavigation { get; set; } = null!;

    public virtual Tratamiento IdTratamientoNavigation { get; set; } = null!;
}
