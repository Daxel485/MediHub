using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly IProveedorService _servicio;
        private readonly IMapper _mapper;

        public ProveedoresController(IProveedorService servicio, IMapper mapper)
        {
            _servicio = servicio;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View(_servicio.ObtenerTodos());
        }

        public ActionResult Crear()
        {
            return View(new ProveedorFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(ProveedorFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(formulario);

            var resultado = _servicio.Registrar(_mapper.Map<ProveedorDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(formulario);
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito ? RedirectToAction("Index") : RedirectToAction("Crear");
        }
    }
}
