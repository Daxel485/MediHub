using System;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Domain.Enums;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    public class ConfiguracionesController : Controller
    {
        private readonly IConfiguracionProveedorService _servicio;
        private readonly IProveedorService _proveedores;
        private readonly IMapper _mapper;

        public ConfiguracionesController(IConfiguracionProveedorService servicio,
            IProveedorService proveedores, IMapper mapper)
        {
            _servicio = servicio;
            _proveedores = proveedores;
            _mapper = mapper;
        }

        // Se accede desde la lista de proveedores con ?idProveedor=; sin filtro muestra todas.
        public ActionResult Index(int? idProveedor)
        {
            if (idProveedor.HasValue)
            {
                var proveedor = _proveedores.ObtenerPorId(idProveedor.Value);
                if (proveedor == null) return HttpNotFound();
                ViewBag.Proveedor = proveedor;
            }
            return View(_servicio.ObtenerTodas(idProveedor));
        }

        public ActionResult Crear(int? idProveedor)
        {
            var formulario = new ConfiguracionFormViewModel { IdProveedor = idProveedor };
            return View(CompletarListas(formulario));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(ConfiguracionFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(CompletarListas(formulario));

            var resultado = _servicio.Registrar(_mapper.Map<ConfiguracionProveedorDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(CompletarListas(formulario));
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito
                ? RedirectToAction("Index", new { idProveedor = formulario.IdProveedor })
                : RedirectToAction("Crear", new { idProveedor = formulario.IdProveedor });
        }

        public ActionResult Editar(int id)
        {
            var configuracion = _servicio.ObtenerPorId(id);
            if (configuracion == null) return HttpNotFound();
            return View(CompletarListas(_mapper.Map<ConfiguracionFormViewModel>(configuracion)));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(ConfiguracionFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(CompletarListas(formulario));

            var resultado = _servicio.Editar(_mapper.Map<ConfiguracionProveedorDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(CompletarListas(formulario));
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito
                ? RedirectToAction("Index", new { idProveedor = formulario.IdProveedor })
                : RedirectToAction("Editar", new { id = formulario.IdConfiguracion });
        }

        private ConfiguracionFormViewModel CompletarListas(ConfiguracionFormViewModel formulario)
        {
            formulario.Proveedores = _proveedores.ObtenerTodos()
                .Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre,
                    Selected = p.IdProveedor == formulario.IdProveedor
                }).ToList();

            formulario.Tipos = Enum.GetValues(typeof(TipoConfiguracion))
                .Cast<TipoConfiguracion>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.Describir(),
                    Selected = (int)t == formulario.TipoConfiguracion
                }).ToList();
            return formulario;
        }
    }
}
