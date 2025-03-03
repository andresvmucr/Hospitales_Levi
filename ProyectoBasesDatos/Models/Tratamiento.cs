using System;
using System.Collections.Generic;

namespace ProyectoBasesDatos.Models;

public partial class Tratamiento
{
    public string Id { get; set; } = null!;

    public decimal Precio { get; set; }

    public string IdCita { get; set; } = null!;

    public virtual Cita IdCitaNavigation { get; set; } = null!;

    public virtual ICollection<TratamientosMed> TratamientosMeds { get; set; } = new List<TratamientosMed>();
}
