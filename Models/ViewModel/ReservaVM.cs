using Microsoft.AspNetCore.Mvc.Rendering;

namespace ByeGolden.Models.ViewModel
{
    public class ReservaVM
    {
        public Reserva vmReserva {  get; set; }
        public List<SelectListItem> vmListaEstados { get; set; }
        public List<SelectListItem> vmListaMetodoPagos { get; set; }
    }
}
