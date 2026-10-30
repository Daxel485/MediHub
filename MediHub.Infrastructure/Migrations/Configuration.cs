using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MediHubContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // Datos de ejemplo para probar los módulos. Solo se insertan si la base está vacía.
        protected override void Seed(MediHubContext contexto)
        {
            if (contexto.Proveedores.Any()) return;

            var hoy = DateTime.Now;
            var inicioDelMes = hoy.Date.AddDays(1 - hoy.Day);
            var mesAnterior = inicioDelMes.AddMonths(-1);

            var drogueria = Proveedor("Droguería Central S.A.", "3101456789", "ventas@drogueriacentral.cr", "22334455", mesAnterior);
            var distribuidora = Proveedor("Distribuidora Médica del Valle", "3101987654", "contacto@medvalle.cr", "24456677", mesAnterior);
            var laboratorio = Proveedor("Laboratorios Andina Ltda.", "3102112233", "licitaciones@andina.cr", "25558899", mesAnterior);
            contexto.Proveedores.AddRange(new[] { drogueria, distribuidora, laboratorio });
            contexto.SaveChanges();

            contexto.Configuraciones.AddRange(new[]
            {
                Configuracion(drogueria, TipoConfiguracion.Plataforma, 5, mesAnterior),
                Configuracion(distribuidora, TipoConfiguracion.Ambas, 8, mesAnterior)
            });

            var amoxicilina = Licitacion("Compra de amoxicilina 500 mg", "10 000 cápsulas, vida útil mínima de 18 meses.", mesAnterior, hoy.AddDays(15));
            var guantes = Licitacion("Guantes de nitrilo talla M", "500 cajas de 100 unidades.", mesAnterior, hoy.AddDays(20));
            var reactivos = Licitacion("Reactivos de hematología", "Requiere cadena de frío.", inicioDelMes, hoy.AddDays(30));
            contexto.Licitaciones.AddRange(new[] { amoxicilina, guantes, reactivos });
            contexto.SaveChanges();

            contexto.Ofertas.AddRange(new List<Oferta>
            {
                Oferta(amoxicilina, drogueria, 1250000m, mesAnterior.AddDays(5), true),
                Oferta(amoxicilina, distribuidora, 1310000m, inicioDelMes, false),
                Oferta(guantes, drogueria, 870000m, inicioDelMes, false),
                Oferta(guantes, laboratorio, 910000m, inicioDelMes, false),
                Oferta(reactivos, distribuidora, 2450000m, inicioDelMes, false)
            });

            contexto.Usuarios.AddRange(new[]
            {
                Usuario(drogueria, "María José", "Rodríguez", "Vargas", "112340567", "mrodriguez@drogueriacentral.cr", mesAnterior),
                Usuario(distribuidora, "Carlos", "Jiménez", "Mora", "204560789", "cjimenez@medvalle.cr", mesAnterior)
            });
            contexto.SaveChanges();
        }

        private static Proveedor Proveedor(string nombre, string cedula, string correo, string telefono, DateTime registro)
        {
            return new Proveedor
            {
                Nombre = nombre,
                Identificacion = cedula,
                CorreoElectronico = correo,
                Telefono = telefono,
                FechaDeRegistro = registro,
                Estado = true
            };
        }

        private static ConfiguracionProveedor Configuracion(Proveedor proveedor, TipoConfiguracion tipo, int comision, DateTime registro)
        {
            return new ConfiguracionProveedor
            {
                IdProveedor = proveedor.IdProveedor,
                TipoConfiguracion = tipo,
                Comision = comision,
                FechaDeRegistro = registro,
                Estado = true
            };
        }

        private static Licitacion Licitacion(string titulo, string descripcion, DateTime registro, DateTime cierre)
        {
            return new Licitacion
            {
                Titulo = titulo,
                Descripcion = descripcion,
                FechaDeRegistro = registro,
                FechaDeCierre = cierre,
                Estado = true
            };
        }

        private static Oferta Oferta(Licitacion licitacion, Proveedor proveedor, decimal monto, DateTime envio, bool sincronizada)
        {
            return new Oferta
            {
                IdLicitacion = licitacion.IdLicitacion,
                IdProveedor = proveedor.IdProveedor,
                Monto = monto,
                Descripcion = "Oferta de " + proveedor.Nombre,
                FechaDeEnvio = envio,
                Sincronizada = sincronizada
            };
        }

        private static Usuario Usuario(Proveedor proveedor, string nombres, string primerApellido,
            string segundoApellido, string identificacion, string correo, DateTime registro)
        {
            return new Usuario
            {
                IdProveedor = proveedor.IdProveedor,
                Nombres = nombres,
                PrimerApellido = primerApellido,
                SegundoApellido = segundoApellido,
                Identificacion = identificacion,
                CorreoElectronico = correo,
                FechaDeRegistro = registro,
                Estado = true
            };
        }
    }
}
