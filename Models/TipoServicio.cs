using System;
using System.Collections.Generic;

namespace ByeGolden.Models;

public partial class TipoServicio
{
    public int IdTipoServicio { get; set; }

    public string NombreTipoServicio { get; set; } = null!;

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
