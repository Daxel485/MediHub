using System.Web.Mvc;
using MediHub.Web.Filters;

namespace MediHub.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new BitacoraExceptionFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
