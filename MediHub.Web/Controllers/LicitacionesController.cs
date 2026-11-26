using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    // TODO Persona 1: restaurar [Authorize(Roles = "Comprador")] cuando Identity este listo.
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class LicitacionesController : Controller
    {
        private readonly ILicitacionService _servicio;
        private readonly IMapper _mapper;
        private readonly IPortalOfertasService _portal;

        private const long MaxPdfBytes = 5 * 1024 * 1024; // 5 MB

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

            var dto = _mapper.Map<LicitacionDto>(formulario);
            var resultado = _servicio.Registrar(dto);

            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(formulario);
            }

            if (!resultado.Exito)
            {
                TempData.MostrarResultado(resultado);
                return View(formulario);
            }

            TempData.MostrarResultado(resultado);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            var dto = _servicio.ObtenerPorId(id);
            if (dto == null) return HttpNotFound();
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SubirTerminos(int id, HttpPostedFileBase archivo)
        {
            if (archivo == null || archivo.ContentLength == 0)
            {
                TempData[ControllerExtensions.ClaveError] = "Debe seleccionar un archivo PDF.";
                return RedirectToAction("Detalle", new { id });
            }

            if (!archivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData[ControllerExtensions.ClaveError] = "Solo se aceptan archivos PDF.";
                return RedirectToAction("Detalle", new { id });
            }

            if (archivo.ContentLength > MaxPdfBytes)
            {
                TempData[ControllerExtensions.ClaveError] = "El archivo no debe superar 5 MB.";
                return RedirectToAction("Detalle", new { id });
            }

            var carpeta = Server.MapPath("~/App_Data/Terminos");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var nombreArchivo = id + "_terminos.pdf";
            var ruta = Path.Combine(carpeta, nombreArchivo);
            archivo.SaveAs(ruta);

            var resultado = _servicio.ActualizarTerminos(id, "~/App_Data/Terminos/" + nombreArchivo);
            TempData.MostrarResultado(resultado);
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AgregarItem(ItemLicitacionViewModel formulario)
        {
            if (!ModelState.IsValid)
            {
                TempData[ControllerExtensions.ClaveError] = "Revise los datos del item.";
                return RedirectToAction("Detalle", new { id = formulario.IdLicitacion });
            }

            var resultado = _servicio.AgregarItem(_mapper.Map<ItemLicitacionDto>(formulario));
            TempData.MostrarResultado(resultado);
            return RedirectToAction("Detalle", new { id = formulario.IdLicitacion });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EliminarItem(int idItem, int idLicitacion)
        {
            var resultado = _servicio.EliminarItem(idItem);
            TempData.MostrarResultado(resultado);
            return RedirectToAction("Detalle", new { id = idLicitacion });
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
    }
}
