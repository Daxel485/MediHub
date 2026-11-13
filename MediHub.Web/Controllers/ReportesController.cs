using System.Web.Mvc;
using MediHub.Application.Interfaces;
using MediHub.Web.Extensions;

namespace MediHub.Web.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IReporteMensualService _servicio;

        public ReportesController(IReporteMensualService servicio)
        {
            _servicio = servicio;
        }

        public ActionResult Index()
        {
            return View(_servicio.ObtenerTodos());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Generar()
        {
            TempData.MostrarResultado(_servicio.GenerarReportes());
            return RedirectToAction("Index");
        }
    }
}
