using System;
using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    public class LicitacionesController : Controller
    {
        private readonly ILicitacionService _servicio;
        private readonly IMapper _mapper;

        public LicitacionesController(ILicitacionService servicio, IMapper mapper)
        {
            _servicio = servicio;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View(_servicio.ObtenerTodas());
        }

        public ActionResult Crear()
        {
            return View(new LicitacionFormViewModel { FechaDeCierre = DateTime.Today.AddDays(15) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(LicitacionFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(formulario);

            var resultado = _servicio.Registrar(_mapper.Map<LicitacionDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(formulario);
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito ? RedirectToAction("Index") : RedirectToAction("Crear");
        }

        public ActionResult Ofertas(int id)
        {
            var licitacion = _servicio.ObtenerPorId(id);
            if (licitacion == null) return HttpNotFound();

            return View(new LicitacionOfertasViewModel
            {
                Licitacion = licitacion,
                Ofertas = _servicio.ObtenerOfertas(id)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sincronizar(int idOferta, int idLicitacion)
        {
            TempData.MostrarResultado(_servicio.SincronizarOferta(idOferta));
            return RedirectToAction("Ofertas", new { id = idLicitacion });
        }
    }
}
