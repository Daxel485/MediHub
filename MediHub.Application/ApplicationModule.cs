using System.Collections.Generic;
using Autofac;
using AutoMapper;
using FluentValidation;
using MediHub.Application.Mappings;
using MediHub.Application.Notificaciones;

namespace MediHub.Application
{
    // Registros de Autofac de la capa de aplicación.
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var ensamblado = ThisAssembly;

            builder.RegisterAssemblyTypes(ensamblado)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            // Fábricas de notificaciones (patrón Factory Method); el servicio las recibe como IEnumerable.
            builder.RegisterAssemblyTypes(ensamblado)
                .AssignableTo<CreadorDeNotificacion>()
                .As<CreadorDeNotificacion>()
                .InstancePerDependency();

            builder.RegisterAssemblyTypes(ensamblado)
                .AsClosedTypesOf(typeof(IValidator<>))
                .SingleInstance();

            // Cada capa registra sus perfiles como Profile; aquí se combinan en una sola configuración.
            builder.RegisterType<ApplicationProfile>().As<Profile>().SingleInstance();
            builder.Register(c =>
                {
                    var perfiles = c.Resolve<IEnumerable<Profile>>();
                    return new MapperConfiguration(cfg =>
                    {
                        foreach (var perfil in perfiles)
                            cfg.AddProfile(perfil);
                    });
                })
                .AsSelf()
                .SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper())
                .As<IMapper>()
                .SingleInstance();
        }
    }
}
