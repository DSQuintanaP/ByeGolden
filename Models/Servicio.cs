using System;
using System.Collections.Generic;

namespace ByeGolden.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public int IdTipoServicio { get; set; }

    public string NomServicio { get; set; } = null!;

    public double Costo { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<DetalleReservaServicio> DetalleReservaServicios { get; set; } = new List<DetalleReservaServicio>();

    public virtual TipoServicio IdTipoServicioNavigation { get; set; } = null!;

    public virtual ICollection<ImagenServicio> ImagenServicios { get; set; } = new List<ImagenServicio>();

    public virtual ICollection<PaqueteServicio> PaqueteServicios { get; set; } = new List<PaqueteServicio>();
}
