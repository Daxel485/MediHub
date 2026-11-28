using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MediHub.Infrastructure.Data;
using MediHub.Infrastructure.Identity;

namespace MediHub.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutofacConfig.Registrar();
            BaseDeDatos.Inicializar();
            // Crea la base si hace falta, los roles y las cuentas de ejemplo.
            IdentityInicializador.Sembrar();
        }
    }
}
