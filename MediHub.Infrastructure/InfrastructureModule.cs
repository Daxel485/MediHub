using Autofac;
using MediHub.Application.Interfaces;
using MediHub.Infrastructure.Data;
using MediHub.Infrastructure.Servicios;

namespace MediHub.Infrastructure
{
    // Registros de Autofac de la capa de infraestructura.
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MediHubContext>().AsSelf().InstancePerRequest();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            // El reloj es inmutable y no depende de la solicitud.
            builder.RegisterType<SystemClock>().As<IClock>().SingleInstance();
        }
    }
}
