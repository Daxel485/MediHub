using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    [Authorize(Roles = "Proveedor")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class PortalProveedorController : Controller
    {
        private readonly IPortalOfertasService _servicio;

        public PortalProveedorController(IPortalOfertasService servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var vigentes = _servicio.ObtenerVigentes(User.ObtenerIdNetUser());
            if (vigentes == null) return new HttpStatusCodeResult(403);
            return View(vigentes);
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            var portal = _servicio.ObtenerPortal(id, User.ObtenerIdNetUser());
            if (portal == null) return HttpNotFound();

            return View(new OfertaFormViewModel
            {
                Portal = portal,
                Detalles = portal.Items.Select(item => new DetalleOfertaFormViewModel
                {
                    IdItemLicitacion = item.IdItemLicitacion
                }).ToList()
            });
        }

        [HttpGet]
        public ActionResult Terminos(int id)
        {
            var pdf = _servicio.AbrirPdf(id, User.ObtenerIdNetUser());
            if (pdf == null) return HttpNotFound();
            return File(pdf, "application/pdf", "terminos-de-referencia.pdf");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Enviar(int id, [Bind(Include = "PlazoEntregaDias,Descripcion,Detalles")] OfertaFormViewModel formulario)
        {
            var idNetUser = User.ObtenerIdNetUser();
            var portal = _servicio.ObtenerPortal(id, idNetUser);
            if (portal == null) return HttpNotFound();
            formulario.Portal = portal;
            if (portal.TieneOfertaPropia) return RedirectToAction("Detalle", new { id });

            var detalles = ConvertirDetalles(formulario);
            if (!ModelState.IsValid || !formulario.PlazoEntregaDias.HasValue)
                return View("Detalle", formulario);

            var resultado = _servicio.EnviarOferta(id, idNetUser, formulario.PlazoEntregaDias.Value,
                formulario.Descripcion, detalles);
            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View("Detalle", formulario);
            }

            TempData.MostrarResultado(resultado);
            return RedirectToAction("Detalle", new { id });
        }

        private IReadOnlyList<DetalleOfertaDto> ConvertirDetalles(OfertaFormViewModel formulario)
        {
            var enviados = formulario.Detalles ?? new List<DetalleOfertaFormViewModel>();
            var items = formulario.Portal.Items;
            var detalles = new List<DetalleOfertaDto>();

            // Se verifica el índice y el identificador para mantener cada campo asociado a su ítem.
            if (enviados.Count != items.Count ||
                enviados.Where((detalle, i) => detalle == null || detalle.IdItemLicitacion != items[i].IdItemLicitacion).Any())
            {
                ModelState.Clear();
                ModelState.AddModelError("", "El formulario de ítems cambió. Revise la licitación e ingrese los precios nuevamente.");
                formulario.Detalles = items.Select(item => new DetalleOfertaFormViewModel
                {
                    IdItemLicitacion = item.IdItemLicitacion
                }).ToList();
                return detalles;
            }

            for (var i = 0; i < enviados.Count; i++)
            {
                var detalle = enviados[i];
                if (!decimal.TryParse(detalle.PrecioUnitario?.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var precio))
                    ModelState.AddModelError($"Detalles[{i}].PrecioUnitario", "Indique un precio unitario válido.");

                detalles.Add(new DetalleOfertaDto
                {
                    IdItemLicitacion = items[i].IdItemLicitacion,
                    PrecioUnitario = precio,
                    FechaVencimientoLote = detalle.FechaVencimientoLote.GetValueOrDefault()
                });
            }

            return detalles;
        }
    }
}
