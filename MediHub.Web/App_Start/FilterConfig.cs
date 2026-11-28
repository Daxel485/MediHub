using System.Web.Mvc;
using MediHub.Web.Filters;

namespace MediHub.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Todo el sitio exige sesión; solo CuentaController marca [AllowAnonymous] el login y el registro.
            // Cada controlador restringe además los roles con [Authorize(Roles = ...)].
            filters.Add(new AuthorizeAttribute());
            filters.Add(new BitacoraExceptionFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
