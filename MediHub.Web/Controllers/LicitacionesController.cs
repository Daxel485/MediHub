using System;
using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    [Authorize(Roles = "Comprador")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class LicitacionesController : Controller
    {
        private readonly ILicitacionService _servicio;
        private readonly IMapper _mapper;
        private readonly IPortalOfertasService _portal;

        public LicitacionesController(ILicitacionService servicio, IMapper mapper, IPortalOfertasService portal)
        {
            _servicio = servicio;
            _mapper = mapper;
            _portal = portal;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_servicio.ObtenerTodas());
        }

        [HttpGet]
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

        [HttpGet]
        public ActionResult Ofertas(int id)
        {
            var comparacion = _portal.Comparar(id);
            if (comparacion == null) return HttpNotFound();
            return View(comparacion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sincronizar(int idOferta, int idLicitacion)
        {
            return MostrarResultado(_portal.Sincronizar(idLicitacion, idOferta), idLicitacion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Adjudicar(int id, int idOferta)
        {
            var comprador = User.ObtenerIdNetUser();
            if (comprador == Guid.Empty) return new HttpStatusCodeResult(403);
            return MostrarResultado(_portal.Adjudicar(id, idOferta, comprador), id);
        }

        private ActionResult MostrarResultado(Resultado resultado, int id)
        {
            TempData.MostrarResultado(resultado);
            return RedirectToAction("Ofertas", new { id });
        }

        // Transiciones del ciclo de vida. El servicio delega en el estado actual de la licitación (patrón State),
        // que rechaza las transiciones no permitidas con un mensaje para el usuario. Adjudicar (arriba) también
        // pasa por el estado desde PortalOfertasService.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Publicar(int id)
        {
            return MostrarResultado(_servicio.Publicar(id), id);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult IniciarEvaluacion(int id)
        {
            return MostrarResultado(_servicio.IniciarEvaluacion(id), id);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cerrar(int id)
        {
            return MostrarResultado(_servicio.Cerrar(id), id);
        }
    }
}
