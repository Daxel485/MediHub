using System.Web.Mvc;
using MediHub.Application.Interfaces;

namespace MediHub.Web.Filters
{
    // Registra en la bitácora cualquier excepción que llegue sin controlar hasta el controlador.
    public class BitacoraExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;

            var bitacora = DependencyResolver.Current.GetService<IBitacoraService>();
            var controlador = (string)filterContext.RouteData.Values["controller"];
            bitacora?.RegistrarError(controlador, filterContext.Exception);
        }
    }
}
