using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ByeGolden.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using ByeGolden.Models.ViewModel;
using QuestPDF;
using QuestPDF.Fluent;

namespace ByeGolden.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ByeGoldenContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReservasController(ByeGoldenContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


        public async Task<IActionResult> Index()
        {
            var reservas = ObtenerTodasLasReservas();

            return View(reservas);
        }


        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.PaquetesDisponibles = ObtenerPaquetesDisponibles();
            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();

            return View(CargarDatosIniciales());

        }


        [HttpPost]
        public IActionResult Crear(Reserva oReserva, string paqueteSeleccionado, string serviciosSeleccionados)
        {

            ViewBag.PaquetesDisponibles = _context.Paquetes.Where(s => s.Estado == true)
                    .ToList(); ;
            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();
            ViewData["Error"] = "True";

            if (string.IsNullOrEmpty(paqueteSeleccionado))
            {
                ModelState.AddModelError("paqueteSeleccionados", "Seleccione un paquete");
                return View(CargarDatosIniciales());
            }

            if (string.IsNullOrEmpty(serviciosSeleccionados) || serviciosSeleccionados == "[]")
            {
                ViewData["ErrorServicio"] = "True";
                return View(CargarDatosIniciales());
            }

            if (!ModelState.IsValid)
            {
                return View(CargarDatosIniciales());
            }

            if (!Existe(oReserva.NroDocumentoCliente))
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no existe");
                return View(CargarDatosIniciales());
            }

            var cliente = _context.Clientes.FirstOrDefault(c => c.NroDocumento == oReserva.NroDocumentoCliente);

            if (cliente.Estado == false)
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente esta inhabilitado");
                return View(CargarDatosIniciales());
            }

            if (cliente.Confirmado == false)
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no ha confirmado su correo");
                return View(CargarDatosIniciales());
            }

            if (!ValidarFechas(oReserva))
            {
                return View(CargarDatosIniciales());
            }

            if (oReserva.Descuento == null)
            {
                oReserva.Descuento = 0;
            }

            _context.Reservas.Add(oReserva);
            _context.SaveChanges();

            var listaPaqueteSeleccionado = JsonConvert.DeserializeObject<List<dynamic>>(paqueteSeleccionado.ToString());

            if (listaPaqueteSeleccionado != null && listaPaqueteSeleccionado.Any())
            {
                var paquetes = listaPaqueteSeleccionado.Select(paquete => new Paquete
                {
                    IdPaquete = Convert.ToInt32(paquete.id),
                    Costo = Convert.ToDouble(paquete.costo)
                }).ToList();

                foreach (var paquete in paquetes)
                {
                    var DetalleReservaPaquete = new DetalleReservaPaquete
                    {
                        IdReserva = oReserva.IdReserva,
                        IdPaquete = paquete.IdPaquete,
                        Costo = paquete.Costo
                    };
                    _context.DetalleReservaPaquetes.Add(DetalleReservaPaquete);
                }
            }

            if (!string.IsNullOrEmpty(serviciosSeleccionados))
            {
                var listaServiciosSeleccionados = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

                if (listaServiciosSeleccionados != null && listaServiciosSeleccionados.Any())
                {
                    var servicios = listaServiciosSeleccionados.Select(servicio => new Servicio
                    {
                        IdServicio = Convert.ToInt32(servicio.id),
                        NomServicio = servicio.nombre.ToString(),
                        Costo = Convert.ToDouble(servicio.costo)
                    }).ToList();

                    for (int i = 0; i < listaServiciosSeleccionados.Count; i++)
                    {
                        if (listaServiciosSeleccionados[i].cantidad == null)
                        {
                            listaServiciosSeleccionados[i].cantidad = 1;
                        }
                        var DetalleReservaServicio = new DetalleReservaServicio
                        {
                            IdReserva = oReserva.IdReserva,
                            IdServicio = listaServiciosSeleccionados[i].id,
                            Costo = listaServiciosSeleccionados[i].costo,
                            Cantidad = listaServiciosSeleccionados[i].cantidad
                        };
                        _context.DetalleReservaServicios.Add(DetalleReservaServicio);
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Reservas");

        }


        [HttpGet]
        public IActionResult Editar(int ReservaId)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(ReservaId);

            var paqueteAsociado = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == ReservaId)
                .Select(p => p.IdPaquete)
                .FirstOrDefault();

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(ReservaId);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(ReservaId);

            ViewBag.PaquetesDisponibles = _context.Paquetes
                .Where(p => p.IdPaquete != paqueteAsociado && p.Estado == true)
                .ToList();

            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();


            return View(CargarDatosReserva(ReservaId));

        }


        [HttpPost]
        public IActionResult Editar(Reserva oReserva, string paqueteSeleccionado, string serviciosSeleccionados)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(oReserva.IdReserva);

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(oReserva.IdReserva);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(oReserva.IdReserva);

            var paqueteAsociado = ObtenerPaqueteAsociado(oReserva.IdReserva);

            ViewBag.PaquetesDisponibles = _context.Paquetes
                .Where(p => p.IdPaquete != paqueteAsociado.IdPaquete && p.Estado == true)
                .ToList();

            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (!Existe(oReserva.NroDocumentoCliente))
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no existe");
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (string.IsNullOrEmpty(paqueteSeleccionado))
            {
                ModelState.AddModelError("paqueteSeleccionados", "Este campo es obligatorio");
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (string.IsNullOrEmpty(serviciosSeleccionados) || serviciosSeleccionados == "[]")
            {
                ViewData["ErrorServicio"] = "True";
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (!ValidarFechas(oReserva))
            {
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (oReserva.Descuento == null)
            {
                oReserva.Descuento = 0;
            }

            var listaAbonos = _context.Abonos
                .Where(a => a.IdReserva == oReserva.IdReserva)
                .ToList();

            var costoOriginalReserva = _context.Reservas
                .Where(r => r.IdReserva == oReserva.IdReserva)
                .Select(r => r.MontoTotal)
                .FirstOrDefault();

            if (oReserva.MontoTotal != costoOriginalReserva && listaAbonos.Count != 0)
            {
                foreach (var abono in listaAbonos)
                {
                    var valorDescuento = oReserva.SubTotal * (1 - (oReserva.Descuento / 100));
                    var nuevoPorcentaje = (abono.SubTotal / valorDescuento) * 100;
                    var porcentajeRedondeado = Math.Floor(nuevoPorcentaje.Value);

                    abono.ValorDeuda = valorDescuento;
                    abono.Porcentaje = porcentajeRedondeado;
                }
            }

            if (oReserva.IdEstadoReserva == 6)
            {
                var oAbono = new AbonoesController(_context, _webHostEnvironment);
                bool validacion = oAbono.ObtenerPendiente(oReserva.IdReserva) == 0;

                if (!validacion)
                {
                    ViewBag.ErrorFinalizacion = "True";
                    return View(CargarDatosReserva(oReserva.IdReserva));
                }

            }

            guardarPaqueteSeleccionado(oReserva, paqueteSeleccionado);
            guardarServiciosSeleccionados(oReserva, serviciosSeleccionados);

            _context.Reservas.Update(oReserva);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Detalles(int ReservaId)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(ReservaId);

            var paqueteAsociado = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == ReservaId)
                .Select(p => p.IdPaquete)
                .FirstOrDefault();

            ViewBag.ServiciosAsociadospaquete = ObtenerServiciosAsociadosPaquete(paqueteAsociado);

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(ReservaId);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(ReservaId);

            return View(CargarDatosReserva(ReservaId));
        }


        public IActionResult AnularReserva(int idReserva)
        {

            var reserva = _context.Reservas
                .FirstOrDefault(r => r.IdReserva == idReserva);

            reserva.IdEstadoReserva = 5;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Metodos 

        public ReservaVM CargarDatosReserva(int? idReserva)
        {

            ReservaVM oReservaVM = new ReservaVM()
            {
                vmReserva = _context.Reservas
                .Where(r => r.IdReserva == idReserva)
                .FirstOrDefault(),
                vmListaEstados = _context.EstadosReservas.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NombreEstadoReserva,
                    Value = reservas.IdEstadoReserva.ToString()
                }).ToList(),
                vmListaMetodoPagos = _context.MetodoPagos.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NomMetodoPago,
                    Value = reservas.IdMetodoPago.ToString()
                }).ToList()
            };

            return oReservaVM;
        }

        public ReservaVM CargarDatosIniciales()
        {

            ReservaVM oReservaVM = new ReservaVM()
            {
                vmReserva = new Reserva(),
                vmListaMetodoPagos = _context.MetodoPagos.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NomMetodoPago,
                    Value = reservas.IdMetodoPago.ToString()
                }).ToList()
            };

            return oReservaVM;
        }

        public bool ValidarFechas(Reserva oReserva)
        {

            if (oReserva.FechaFinalizacion < oReserva.FechaInicio)
            {
                ModelState.AddModelError("oReserva.FechaFinalizacion", "La fecha de finalizacion debe ser posterior a la fecha de inicio");
                return false;
            }
            else if (oReserva.FechaReserva >= oReserva.FechaInicio)
            {
                ModelState.AddModelError("oReserva.FechaInicio", "La fecha de inicio debe ser mayor que la fecha de la reserva");
                return false;
            }

            return true;
        }

        public IActionResult BuscarCliente(string searchTerm)
        {
            var informacionCliente = _context.Clientes
                .Include(s => s.IdTipoDocumentoNavigation)
                .Where(s => s.NroDocumento == searchTerm)
                .FirstOrDefault();

            if (informacionCliente == null)
            {
                return Json(new
                {
                    nombre = "",
                    apellido = "",
                    tipoDocumento = "",
                    correo = "",
                    celular = "",
                });
            }
            else if (informacionCliente.Estado == false)
            {
                return Json(new
                {
                    estado = "false",
                });
            }
            else
            {
                return Json(new
                {
                    nombre = informacionCliente.Nombres,
                    apellido = informacionCliente.Apellidos,
                    tipoDocumento = informacionCliente.IdTipoDocumentoNavigation.NomTipoDcumento,
                    correo = informacionCliente.Correo,
                    celular = informacionCliente.Celular
                });
            }
        }

        public bool Existe(string cliente)
        {
            return _context.Clientes.Any(c => c.NroDocumento == cliente);
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Reserva> resultados;

            if (string.IsNullOrEmpty(searchTerm))
            {
                resultados = ObtenerTodasLasReservas();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        public List<int?> CantidadesServicios(int? reservaId)
        {
            var serviciosAsociados = ObtenerServiciosAsociados(reservaId);

            List<int?> listaCantidades = new List<int?>();

            foreach (var servicio in serviciosAsociados)
            {
                var cantidad = _context.DetalleReservaServicios
                    .Where(drs => drs.IdServicio == servicio.IdServicio && drs.IdReserva == reservaId)
                    .Select(drs => drs.Cantidad)
                    .FirstOrDefault();

                listaCantidades.Add(cantidad);
            }

            return listaCantidades;

        }

        public List<Reserva> ObtenerTodasLasReservas()
        {
            var todasLasReservas = _context.Reservas
                .Include(e => e.IdEstadoReservaNavigation)
                .Include(e => e.MetodoPagoNavigation)
                .Include(e => e.NroDocumentoClienteNavigation)
                    .ThenInclude(e => e.IdTipoDocumentoNavigation)
                .Include(e => e.NroDocumentoUsuarioNavigation)
                .Include(e => e.DetalleReservaPaquetes)
                    .ThenInclude(drp => drp.IdPaqueteNavigation)
                        .ThenInclude(p => p.IdHabitacionNavigation)
                            .ThenInclude(h => h.IdTipoHabitacionNavigation)
                .ToList();

            return todasLasReservas;
        }

        public List<Reserva> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            if (DateOnly.TryParse(searchTerm, out DateOnly fecha))
            {
                var resultados = _context.Reservas
                    .Include(e => e.IdEstadoReservaNavigation)
                    .Include(e => e.MetodoPagoNavigation)
                    .Include(e => e.NroDocumentoClienteNavigation)
                    .Include(e => e.NroDocumentoUsuarioNavigation)
                    .Where(e => e.FechaReserva == fecha)
                    .ToList();

                return resultados;
            }
            else
            {
                var resultados = _context.Reservas
                    .Include(e => e.IdEstadoReservaNavigation)
                    .Include(e => e.MetodoPagoNavigation)
                    .Include(e => e.NroDocumentoClienteNavigation)
                    .Include(e => e.NroDocumentoUsuarioNavigation)
                    .Where(e => e.NroDocumentoCliente.Contains(searchTerm))
                    .ToList();

                return resultados;
            }
        }

        public Paquete ObtenerPaqueteAsociado(int? reservaId)
        {
            return _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == reservaId)
                .Select(p => p.IdPaqueteNavigation)
                .FirstOrDefault();
        }

        public List<DetalleReservaServicio> ObtenerServiciosAsociados(int? ReservaId)
        {
            return _context.DetalleReservaServicios
                .Where(drs => drs.IdReserva == ReservaId)
                .Include(drs => drs.IdServicioNavigation)
                .ToList();
        }

        public List<Servicio> ObtenerServiciosAsociadosPaquete(int? idPaquete)
        {
            return _context.PaqueteServicios
                .Where(p => p.IdPaquete == idPaquete)
                .Select(p => p.IdServicioNavigation)
                .ToList();
        }

        public List<Paquete> ObtenerPaquetesDisponibles()
        {

            var habitacionesReservadas = _context.DetalleReservaPaquetes
                .Where(drp => drp.IdReservaNavigation.IdEstadoReserva != 6 && drp.IdReservaNavigation.IdEstadoReserva != 5)
                .Select(drp => drp.IdPaqueteNavigation.IdHabitacion)
                .Distinct()
                .ToList();

            var paquetesDisponibles = _context.Paquetes
                .Where(p => !habitacionesReservadas.Contains(p.IdHabitacion) && p.Estado == true)
                .ToList();

            return paquetesDisponibles;
        }

        public IActionResult ObtenerInfoBasicaPaquete(int reservaId)
        {
            var detalle = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == reservaId)
                .FirstOrDefault();

            return Json(new
            {
                costo = detalle.Costo
            });
        }

        public IActionResult ObtenerCostoServicio(int servicioId)
        {
            var costoServicio = _context.Servicios
                                        .Where(s => s.IdServicio == servicioId)
                                        .Select(s => s.Costo)
                                        .FirstOrDefault();

            return Json(new { costo = costoServicio });
        }

        public IActionResult ObtenerInfoPaquete(int paqueteId)
        {
            var Informacionpaquete = _context.Paquetes
                                  .Include(s => s.IdHabitacionNavigation)
                                    .ThenInclude(s => s.IdTipoHabitacionNavigation)
                                  .Where(s => s.IdPaquete == paqueteId)
                                  .FirstOrDefault();

            return Json(new
            {
                nombre = Informacionpaquete.NomPaquete,
                costo = Informacionpaquete.Costo,
                habitacion = Informacionpaquete.IdHabitacionNavigation.Nombre,
                nroPersonas = Informacionpaquete.IdHabitacionNavigation.IdTipoHabitacionNavigation.NumeroPersonas,
            });

        }

        public IActionResult ObtenerServiciosPaquete(int paqueteId)
        {
            var serviciosPaquete = _context.PaqueteServicios
                .Where(e => e.IdPaquete == paqueteId)
                .Include(e => e.IdServicioNavigation)
                .ToList();

            var servicioData = serviciosPaquete.Select(e => new
            {
                nombre = e.IdServicioNavigation.NomServicio,
                costo = e.Costo
            });

            return Json(servicioData);
        }

        public bool guardarPaqueteSeleccionado(Reserva oReserva, string paqueteSeleccionado)
        {
            var PaqueteSeleccionadoObj = JsonConvert.DeserializeObject<List<dynamic>>(paqueteSeleccionado.ToString());

            if (PaqueteSeleccionadoObj != null && PaqueteSeleccionadoObj.Count > 0)
            {

                var primerPaquete = PaqueteSeleccionadoObj[0];

                var nuevoPaquete = new Paquete()
                {
                    IdPaquete = primerPaquete.id,
                    Costo = primerPaquete.costo
                };

                var paqueteOriginal = _context.DetalleReservaPaquetes
                    .Where(drp => drp.IdReserva == oReserva.IdReserva)
                    .Select(drp => drp.IdPaqueteNavigation)
                    .FirstOrDefault();

                var detallePaqueteExistente = _context.DetalleReservaPaquetes
                   .FirstOrDefault(d => d.IdReserva == oReserva.IdReserva && d.IdPaquete == paqueteOriginal.IdPaquete);


                if (nuevoPaquete.IdPaquete != paqueteOriginal.IdPaquete)
                {

                    detallePaqueteExistente.IdPaquete = nuevoPaquete.IdPaquete;
                    detallePaqueteExistente.Costo = nuevoPaquete.Costo;

                    _context.DetalleReservaPaquetes.Update(detallePaqueteExistente);

                }

                return true;

            }

            return false;
        }

        public bool guardarServiciosSeleccionados(Reserva oReserva, string serviciosSeleccionados)
        {
            var ServiciosSeleccionadosObj = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

            if (ServiciosSeleccionadosObj != null && ServiciosSeleccionadosObj.Any())
            {

                var serviciosNuevos = new List<Servicio>();
                var cantidadeServiciosNuevos = new List<int>();

                for (var i = 0; i < ServiciosSeleccionadosObj.Count; i++)
                {
                    var servicio = new Servicio()
                    {
                        IdServicio = Convert.ToInt32(ServiciosSeleccionadosObj[i].id),
                        NomServicio = ServiciosSeleccionadosObj[i].nombre.ToString(),
                        Costo = Convert.ToDouble(ServiciosSeleccionadosObj[i].costo)
                    };

                    var cantidad = ServiciosSeleccionadosObj[i].cantidad;

                    serviciosNuevos.Add(servicio);

                    if (cantidad == null)
                    {
                        cantidad = 1;
                    }

                    cantidadeServiciosNuevos.Add((int)cantidad);
                }

                var serviciosOriginales = _context.DetalleReservaServicios
                    .Where(drs => drs.IdReserva == oReserva.IdReserva)
                    .Select(drs => drs.IdServicioNavigation.IdServicio)
                    .ToList();

                var serviciosAEliminar = serviciosOriginales.Except(serviciosNuevos.Select(s => s.IdServicio)).ToList();
                var serviciosAAgregar = serviciosNuevos.Select(s => s.IdServicio).Except(serviciosOriginales).ToList();

                if (serviciosAEliminar.Count != 0)
                {

                    foreach (var servicioEliminar in serviciosAEliminar)
                    {

                        var detalle = _context.DetalleReservaServicios
                            .Where(drs => drs.IdReserva == oReserva.IdReserva && drs.IdServicio == servicioEliminar)
                            .FirstOrDefault();

                        _context.DetalleReservaServicios.Remove(detalle);

                    }

                }

                if (serviciosAAgregar.Count != 0)
                {

                    for (var i = 0; i < serviciosAAgregar.Count; i++)
                    {
                        var detalle = new DetalleReservaServicio()
                        {
                            IdReserva = oReserva.IdReserva,
                            IdServicio = serviciosAAgregar[i],
                            Costo = _context.Servicios
                                        .Where(s => s.IdServicio == serviciosAAgregar[i])
                                        .Select(s => s.Costo)
                                        .FirstOrDefault(),
                            Cantidad = cantidadeServiciosNuevos[i]
                        };

                        _context.DetalleReservaServicios.Add(detalle);
                    }

                }

                _context.SaveChanges();

                for (var i = 0; i < serviciosNuevos.Count; i++)
                {
                    var detalle = new DetalleReservaServicio()
                    {

                        IdReserva = oReserva.IdReserva,
                        IdServicio = serviciosNuevos[i].IdServicio,
                        Costo = serviciosNuevos[i].Costo,
                        Cantidad = cantidadeServiciosNuevos[i]

                    };

                    var detalleExistente = _context.DetalleReservaServicios
                        .FirstOrDefault(drs => drs.IdReserva == oReserva.IdReserva && drs.IdServicio == detalle.IdServicio);

                    if (detalleExistente != null)
                    {
                        detalleExistente.Cantidad = detalle.Cantidad;

                        _context.SaveChanges();
                    }
                }

                return true;

            }
            else
            {
                var serviciosAdicionales = _context.DetalleReservaServicios
                    .Where(d => d.IdReserva == oReserva.IdReserva)
                    .ToList();

                _context.DetalleReservaServicios.RemoveRange(serviciosAdicionales);

                return true;
            }
        }

        

            /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            //// GET: Reservas
            //public async Task<IActionResult> Index()
            //{
            //    var byeGoldenContext = _context.Reservas.Include(r => r.IdEstadoReservaNavigation).Include(r => r.MetodoPagoNavigation).Include(r => r.NroDocumentoClienteNavigation).Include(r => r.NroDocumentoUsuarioNavigation);
            //    return View(await byeGoldenContext.ToListAsync());
            //}

            //// GET: Reservas/Details/5
            //public async Task<IActionResult> Details(int? id)
            //{
            //    if (id == null)
            //    {
            //        return NotFound();
            //    }

            //    var reserva = await _context.Reservas
            //        .Include(r => r.IdEstadoReservaNavigation)
            //        .Include(r => r.MetodoPagoNavigation)
            //        .Include(r => r.NroDocumentoClienteNavigation)
            //        .Include(r => r.NroDocumentoUsuarioNavigation)
            //        .FirstOrDefaultAsync(m => m.IdReserva == id);
            //    if (reserva == null)
            //    {
            //        return NotFound();
            //    }

            //    return View(reserva);
            //}

            //// GET: Reservas/Create
            //public IActionResult Create()
            //{
            //    ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "IdEstadoReserva");
            //    ViewData["MetodoPago"] = new SelectList(_context.MetodoPagos, "IdMetodoPago", "IdMetodoPago");
            //    ViewData["NroDocumentoCliente"] = new SelectList(_context.Clientes, "NroDocumento", "NroDocumento");
            //    ViewData["NroDocumentoUsuario"] = new SelectList(_context.Usuarios, "NroDocumento", "NroDocumento");
            //    return View();
            //}

            //// POST: Reservas/Create
            //// To protect from overposting attacks, enable the specific properties you want to bind to.
            //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            //[HttpPost]
            //[ValidateAntiForgeryToken]
            //public async Task<IActionResult> Create([Bind("IdReserva,NroDocumentoCliente,NroDocumentoUsuario,FechaReserva,FechaInicio,FechaFinalizacion,SubTotal,Descuento,Iva,MontoTotal,MetodoPago,NroPersonas,IdEstadoReserva")] Reserva reserva)
            //{
            //    if (ModelState.IsValid)
            //    {
            //        _context.Add(reserva);
            //        await _context.SaveChangesAsync();
            //        return RedirectToAction(nameof(Index));
            //    }
            //    ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "IdEstadoReserva", reserva.IdEstadoReserva);
            //    ViewData["MetodoPago"] = new SelectList(_context.MetodoPagos, "IdMetodoPago", "IdMetodoPago", reserva.MetodoPago);
            //    ViewData["NroDocumentoCliente"] = new SelectList(_context.Clientes, "NroDocumento", "NroDocumento", reserva.NroDocumentoCliente);
            //    ViewData["NroDocumentoUsuario"] = new SelectList(_context.Usuarios, "NroDocumento", "NroDocumento", reserva.NroDocumentoUsuario);
            //    return View(reserva);
            //}

            //// GET: Reservas/Edit/5
            //public async Task<IActionResult> Edit(int? id)
            //{
            //    if (id == null)
            //    {
            //        return NotFound();
            //    }

            //    var reserva = await _context.Reservas.FindAsync(id);
            //    if (reserva == null)
            //    {
            //        return NotFound();
            //    }
            //    ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "IdEstadoReserva", reserva.IdEstadoReserva);
            //    ViewData["MetodoPago"] = new SelectList(_context.MetodoPagos, "IdMetodoPago", "IdMetodoPago", reserva.MetodoPago);
            //    ViewData["NroDocumentoCliente"] = new SelectList(_context.Clientes, "NroDocumento", "NroDocumento", reserva.NroDocumentoCliente);
            //    ViewData["NroDocumentoUsuario"] = new SelectList(_context.Usuarios, "NroDocumento", "NroDocumento", reserva.NroDocumentoUsuario);
            //    return View(reserva);
            //}

            //// POST: Reservas/Edit/5
            //// To protect from overposting attacks, enable the specific properties you want to bind to.
            //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            //[HttpPost]
            //[ValidateAntiForgeryToken]
            //public async Task<IActionResult> Edit(int id, [Bind("IdReserva,NroDocumentoCliente,NroDocumentoUsuario,FechaReserva,FechaInicio,FechaFinalizacion,SubTotal,Descuento,Iva,MontoTotal,MetodoPago,NroPersonas,IdEstadoReserva")] Reserva reserva)
            //{
            //    if (id != reserva.IdReserva)
            //    {
            //        return NotFound();
            //    }

            //    if (ModelState.IsValid)
            //    {
            //        try
            //        {
            //            _context.Update(reserva);
            //            await _context.SaveChangesAsync();
            //        }
            //        catch (DbUpdateConcurrencyException)
            //        {
            //            if (!ReservaExists(reserva.IdReserva))
            //            {
            //                return NotFound();
            //            }
            //            else
            //            {
            //                throw;
            //            }
            //        }
            //        return RedirectToAction(nameof(Index));
            //    }
            //    ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "IdEstadoReserva", reserva.IdEstadoReserva);
            //    ViewData["MetodoPago"] = new SelectList(_context.MetodoPagos, "IdMetodoPago", "IdMetodoPago", reserva.MetodoPago);
            //    ViewData["NroDocumentoCliente"] = new SelectList(_context.Clientes, "NroDocumento", "NroDocumento", reserva.NroDocumentoCliente);
            //    ViewData["NroDocumentoUsuario"] = new SelectList(_context.Usuarios, "NroDocumento", "NroDocumento", reserva.NroDocumentoUsuario);
            //    return View(reserva);
            //}

            //// GET: Reservas/Delete/5
            //public async Task<IActionResult> Delete(int? id)
            //{
            //    if (id == null)
            //    {
            //        return NotFound();
            //    }

            //    var reserva = await _context.Reservas
            //        .Include(r => r.IdEstadoReservaNavigation)
            //        .Include(r => r.MetodoPagoNavigation)
            //        .Include(r => r.NroDocumentoClienteNavigation)
            //        .Include(r => r.NroDocumentoUsuarioNavigation)
            //        .FirstOrDefaultAsync(m => m.IdReserva == id);
            //    if (reserva == null)
            //    {
            //        return NotFound();
            //    }

            //    return View(reserva);
            //}

            //// POST: Reservas/Delete/5
            //[HttpPost, ActionName("Delete")]
            //[ValidateAntiForgeryToken]
            //public async Task<IActionResult> DeleteConfirmed(int id)
            //{
            //    var reserva = await _context.Reservas.FindAsync(id);
            //    if (reserva != null)
            //    {
            //        _context.Reservas.Remove(reserva);
            //    }

            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}

            //private bool ReservaExists(int id)
            //{
            //    return _context.Reservas.Any(e => e.IdReserva == id);
            //}
        
    }
}
