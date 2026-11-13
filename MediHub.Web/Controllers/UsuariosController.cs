using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _servicio;
        private readonly IProveedorService _proveedores;
        private readonly IMapper _mapper;

        public UsuariosController(IUsuarioService servicio, IProveedorService proveedores, IMapper mapper)
        {
            _servicio = servicio;
            _proveedores = proveedores;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View(_servicio.ObtenerTodos());
        }

        public ActionResult Crear()
        {
            return View(CompletarListas(new UsuarioFormViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(UsuarioFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(CompletarListas(formulario));

            var resultado = _servicio.Registrar(_mapper.Map<UsuarioDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(CompletarListas(formulario));
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito ? RedirectToAction("Index") : RedirectToAction("Crear");
        }

        public ActionResult Editar(int id)
        {
            var usuario = _servicio.ObtenerPorId(id);
            if (usuario == null) return HttpNotFound();
            return View(_mapper.Map<UsuarioFormViewModel>(usuario));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(UsuarioFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(formulario);

            var resultado = _servicio.Editar(_mapper.Map<UsuarioDto>(formulario));
            if (resultado.TieneErroresDeValidacion)
            {
                ModelState.AgregarErrores(resultado);
                return View(formulario);
            }

            TempData.MostrarResultado(resultado);
            return resultado.Exito
                ? RedirectToAction("Index")
                : RedirectToAction("Editar", new { id = formulario.IdUsuario });
        }

        private UsuarioFormViewModel CompletarListas(UsuarioFormViewModel formulario)
        {
            formulario.Proveedores = _proveedores.ObtenerTodos()
                .Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre,
                    Selected = p.IdProveedor == formulario.IdProveedor
                }).ToList();
            return formulario;
        }
    }
}
