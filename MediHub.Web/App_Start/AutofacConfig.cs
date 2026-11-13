using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using MediHub.Application;
using MediHub.Infrastructure;
using MediHub.Web.Mappings;

namespace MediHub.Web
{
    public static class AutofacConfig
    {
        public static void Registrar()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterFilterProvider();
            builder.RegisterModule<ApplicationModule>();
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterType<WebProfile>().As<Profile>().SingleInstance();

            var contenedor = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(contenedor));
        }
    }
}
