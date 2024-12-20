﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ByeGolden.Models;
using Microsoft.AspNetCore.Authorization;

namespace ByeGolden.Controllers
{
    public class AbonoesController : Controller
    {
        private readonly ByeGoldenContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public AbonoesController(ByeGoldenContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        
        public IActionResult Index(int reservaId)
        {
            var abonos = _context.Abonos
                .Include(m => m.IdReservaNavigation)
                .Where(m => m.IdReserva == reservaId)
                .ToList();

            ViewBag.reservaId = reservaId;

            var pendiente = ObtenerPendiente(reservaId);

            if (pendiente <= 0)
            {
                ViewData["botonInhabilitado"] = "true";
            }

            var reservaEstado = _context.Reservas
                .Where(r => r.IdReserva == reservaId)
                .Select(r => r.IdEstadoReserva)
                .FirstOrDefault();

            if (reservaEstado == 5)
            {
                ViewData["anulado"] = "true";
            }
            else if (reservaEstado == 6)
            {
                ViewData["finalizada"] = "true";
            }

            return View(abonos);
        }

        
        [HttpGet]
        public IActionResult Create(int reservaId)
        {

            ViewData["total"] = ObtenerTotalReserva(reservaId);
            ViewData["pendiente"] = ObtenerPendiente(reservaId);

            ViewBag.reservaId = reservaId;
            ViewBag.Reserva = ObtenerReserva(reservaId);
            ViewBag.PaqueteReserva = ObtenerPaqueteReserva(reservaId);
            ViewBag.ServiciosReserva = ObtenerServiciosReserva(reservaId);

            return View(CargarIformacionIncial());
        }

        
        [HttpPost]
        public IActionResult Create(Abono oAbono, List<IFormFile> Imagenes)
        {
            if (oAbono.Estado == null || oAbono.SubTotal == 0)
            {
                if (oAbono.SubTotal == 0)
                {
                    ModelState.AddModelError("subTotal", "Este campo es obligatorio");
                }

                ViewData["Error"] = "True";
                ViewData["total"] = ObtenerTotalReserva(oAbono.IdReserva);
                ViewData["pendiente"] = ObtenerPendiente(oAbono.IdReserva);

                ViewBag.reservaId = oAbono.IdReserva;
                ViewBag.PaqueteReserva = ObtenerPaqueteReserva(oAbono.IdReserva);
                ViewBag.ServiciosReserva = ObtenerServiciosReserva(oAbono.IdReserva);

                return View(CargarIformacionIncial());
            }

            if ((oAbono.Porcentaje < 50 && !ExistePrimerAbono(oAbono.IdReserva)) || oAbono.SubTotal > oAbono.Pendiente)
            {
                if (oAbono.Porcentaje < 50 && !ExistePrimerAbono(oAbono.IdReserva))
                {

                    ModelState.AddModelError("subTotal", "El primer abono no puede ser menor al 50%");

                }
                else
                {
                    ModelState.AddModelError("subTotal", "No puedes abonar un valor mayor al pendiente");
                }

                ViewData["Error"] = "True";
                ViewData["total"] = ObtenerTotalReserva(oAbono.IdReserva);
                ViewData["pendiente"] = ObtenerPendiente(oAbono.IdReserva);

                ViewBag.reservaId = oAbono.IdReserva;
                ViewBag.PaqueteReserva = ObtenerPaqueteReserva(oAbono.IdReserva);
                ViewBag.ServiciosReserva = ObtenerServiciosReserva(oAbono.IdReserva);

                return View(CargarIformacionIncial());
            }

            _context.Abonos.Add(oAbono);
            _context.SaveChanges();

            foreach (var imagenes in Imagenes)
            {
                if (imagenes != null && imagenes.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imagenes.CopyTo(stream);

                        var webRootPath = _webHostEnvironment.WebRootPath;
                        var nuevoNombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagenes.FileName)}";

                        var imagePath = Path.Combine(webRootPath, "imagenes", nuevoNombreArchivo);
                        System.IO.File.WriteAllBytes(imagePath, stream.ToArray());

                        var imagen = new Imagene
                        {
                            UrlImagen = $"/imagenes/{nuevoNombreArchivo}"
                        };

                        _context.Imagenes.Add(imagen);
                        _context.SaveChanges();

                        var imagenAbono = new ImagenAbono
                        {
                            IdImagen = imagen.IdImagen,
                            IdAbono = oAbono.IdAbono
                        };

                        _context.ImagenAbonos.Add(imagenAbono);
                        _context.SaveChanges();
                    }
                }
            }

            var reserva = _context.Reservas
                .FirstOrDefault(r => r.IdReserva == oAbono.IdReserva);

            if (reserva.IdEstadoReserva == 1)
            {
                reserva.IdEstadoReserva = 2;

                _context.Reservas.Update(reserva);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Abono", new { reservaId = oAbono.IdReserva });
        }

        
        public IActionResult Details(int idAbono, int reservaId)
        {
            ViewData["total"] = ObtenerTotalReserva(reservaId);
            ViewData["pendiente"] = ObtenerPendiente(reservaId);

            ViewBag.reservaId = reservaId;
            ViewBag.ImagenesAsociadas = ObtenerImagenesAsociadas(idAbono);
            ViewBag.PaqueteReserva = ObtenerPaqueteReserva(reservaId);
            ViewBag.ServiciosReserva = ObtenerServiciosReserva(reservaId);

            return View(CargarInformacionEditar(idAbono));
        }

        
        public IActionResult anularAbono(int idAbono)
        {
            var abono = _context.Abonos
                .FirstOrDefault(a => a.IdAbono == idAbono);

            abono.Estado = false;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // Metodos

        public List<string?> ObtenerImagenesAsociadas(int? idAbono)
        {
            return _context.ImagenAbonos
                .Where(ip => ip.IdAbono == idAbono)
                .Select(ip => ip.IdImagenNavigation.UrlImagen)
                .ToList();
        }

        public Abono CargarIformacionIncial()
        {

            Abono oAbono = new Abono();

            return oAbono;

        }

        public Abono CargarInformacionEditar(int idAbono)
        {
            Abono oAbono = _context.Abonos.Find(idAbono);

            return oAbono;
        }

        public bool ExistePrimerAbono(int? reservaId)
        {
            return _context.Abonos.Any(a => a.IdReserva == reservaId && a.Estado == true);
        }

        public double? ObtenerTotalReserva(int? idReserva)
        {
            var subtotalReserva = _context.Reservas
                .Where(r => r.IdReserva == idReserva)
                .Select(r => r.SubTotal)
                .FirstOrDefault();

            var descuento = _context.Reservas
                .Where(r => r.IdReserva == idReserva)
                .Select(r => r.Descuento)
                .FirstOrDefault();

            var totalReserva = subtotalReserva * (1 - (descuento / 100));

            return totalReserva;
        }

        public double? ObtenerPendiente(int? idReserva)
        {
            var ListaPagosAbono = _context.Abonos
                .Where(a => a.IdReserva == idReserva && a.Estado == true)
                .Select(a => a.SubTotal)
                .ToList();

            double? abonado = 0;

            if (ListaPagosAbono != null)
            {
                foreach (var pago in ListaPagosAbono)
                {
                    abonado += pago;
                }
            }
            else
            {
                abonado = 0;
            }

            var totalReserva = ObtenerTotalReserva(idReserva);

            var pendiente = totalReserva - abonado;

            return pendiente;
        }

        public DetalleReservaPaquete ObtenerPaqueteReserva(int? idReserva)
        {
            var Paquete = _context.DetalleReservaPaquetes
                .Include(m => m.IdPaqueteNavigation)
                .Where(m => m.IdReserva == idReserva)
                .FirstOrDefault();

            return Paquete;
        }

        public List<DetalleReservaServicio> ObtenerServiciosReserva(int? idReserva)
        {
            var Servicios = _context.DetalleReservaServicios
                .Include(m => m.IdServicioNavigation)
                    .ThenInclude(s => s.IdTipoServicioNavigation)
                .Where(m => m.IdReserva == idReserva)
                .ToList();

            return Servicios;
        }

        public Reserva ObtenerReserva(int reservaId)
        {
            return _context.Reservas.FirstOrDefault(r => r.IdReserva == reservaId);
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Abono> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosAbonos();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Abono> ObtenerTodosLosAbonos()
        {
            var todosLosAbonos = _context.Abonos
                .Include(m => m.IdReservaNavigation)
                .ToList();

            return todosLosAbonos;
        }

        private List<Abono> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            if (DateOnly.TryParse(searchTerm, out DateOnly fecha))
            {
                var resultados = _context.Abonos
                                    .Include(e => e.IdReservaNavigation)
                                    .Where(e => e.FechaAbono == fecha)
                                    .ToList();
                return resultados;
            }
            else
            {
                var resultados = _context.Abonos
                                    .Include(e => e.IdReservaNavigation)
                                    .Where(e => e.IdAbono.ToString().Contains(searchTerm))
                                    .ToList();
                return resultados;
            }

        }

        /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        //// GET: Abonoes
        //public async Task<IActionResult> Index()
        //{
        //    var byeGoldenContext = _context.Abonos.Include(a => a.IdReservaNavigation);
        //    return View(await byeGoldenContext.ToListAsync());
        //}

        //// GET: Abonoes/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var abono = await _context.Abonos
        //        .Include(a => a.IdReservaNavigation)
        //        .FirstOrDefaultAsync(m => m.IdAbono == id);
        //    if (abono == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(abono);
        //}

        //// GET: Abonoes/Create
        //public IActionResult Create()
        //{
        //    ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva");
        //    return View();
        //}

        //// POST: Abonoes/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("IdAbono,IdReserva,FechaAbono,ValorDeuda,Porcentaje,Pendiente,SubTotal,Iva,CantAbono,Estado")] Abono abono)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(abono);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", abono.IdReserva);
        //    return View(abono);
        //}

        //// GET: Abonoes/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var abono = await _context.Abonos.FindAsync(id);
        //    if (abono == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", abono.IdReserva);
        //    return View(abono);
        //}

        //// POST: Abonoes/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("IdAbono,IdReserva,FechaAbono,ValorDeuda,Porcentaje,Pendiente,SubTotal,Iva,CantAbono,Estado")] Abono abono)
        //{
        //    if (id != abono.IdAbono)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(abono);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!AbonoExists(abono.IdAbono))
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
        //    ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", abono.IdReserva);
        //    return View(abono);
        //}

        //// GET: Abonoes/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var abono = await _context.Abonos
        //        .Include(a => a.IdReservaNavigation)
        //        .FirstOrDefaultAsync(m => m.IdAbono == id);
        //    if (abono == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(abono);
        //}

        //// POST: Abonoes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var abono = await _context.Abonos.FindAsync(id);
        //    if (abono != null)
        //    {
        //        _context.Abonos.Remove(abono);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool AbonoExists(int id)
        //{
        //    return _context.Abonos.Any(e => e.IdAbono == id);
        //}
    }
}
