using System;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Constants;
using MediHub.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MediHub.Infrastructure.Identity
{
    // Crea las tablas de Identity, los tres roles y las cuentas de ejemplo (solo para desarrollo).
    public static class IdentityInicializador
    {
        private sealed class CuentaDeEjemplo
        {
            public string Correo;
            public string Nombre;
            public string Contrasena;
            public string Rol;
        }

        // Cuentas sin proveedor asociado.
        private static readonly CuentaDeEjemplo[] CuentasInternas =
        {
            new CuentaDeEjemplo { Correo = "admin@medihub.cr", Nombre = "Administrador MediHub", Contrasena = "Admin123*", Rol = Roles.Administrador },
            new CuentaDeEjemplo { Correo = "comprador@medihub.cr", Nombre = "Compras Clínica San Rafael", Contrasena = "Comprador123*", Rol = Roles.Comprador }
        };

        // Usuarios de proveedor que ya vienen en los datos de ejemplo de MediHubContext (se buscan por correo).
        private static readonly CuentaDeEjemplo[] CuentasDeProveedor =
        {
            new CuentaDeEjemplo { Correo = "mrodriguez@drogueriacentral.cr", Contrasena = "Proveedor123*", Rol = Roles.Proveedor },
            new CuentaDeEjemplo { Correo = "cjimenez@medvalle.cr", Contrasena = "Proveedor123*", Rol = Roles.Proveedor }
        };

        public static void ConfigurarInicializador()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, IdentityConfiguration>());
        }

        public static void Sembrar()
        {
            // Primero la base de MediHub (crea la base y los datos de ejemplo), luego las tablas de Identity.
            using (var medihub = new MediHubContext())
            {
                medihub.Database.Initialize(false);

                using (var identity = new ApplicationDbContext())
                {
                    identity.Database.Initialize(false);
                    SembrarRoles(identity);
                    SembrarCuentas(identity, medihub);
                }
            }
        }

        private static void SembrarRoles(ApplicationDbContext identity)
        {
            var roles = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(identity));
            foreach (var rol in Roles.Todos)
            {
                if (!roles.RoleExists(rol))
                    roles.Create(new IdentityRole(rol));
            }
        }

        private static void SembrarCuentas(ApplicationDbContext identity, MediHubContext medihub)
        {
            var usuarios = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(identity))
            {
                UserLockoutEnabledByDefault = true
            };

            foreach (var cuenta in CuentasInternas)
                CrearCuenta(usuarios, cuenta.Correo, cuenta.Nombre, cuenta.Contrasena, cuenta.Rol);

            foreach (var cuenta in CuentasDeProveedor)
            {
                var usuario = medihub.Usuarios.FirstOrDefault(u => u.CorreoElectronico == cuenta.Correo);
                if (usuario == null || usuario.IdNetUser.HasValue) continue;

                var nombre = usuario.Nombres + " " + usuario.PrimerApellido + " " + usuario.SegundoApellido;
                var cuentaCreada = CrearCuenta(usuarios, cuenta.Correo, nombre, cuenta.Contrasena, cuenta.Rol);
                if (cuentaCreada == null) continue;

                // Enlaza la cuenta de Identity con el registro de Usuario.
                usuario.IdNetUser = Guid.Parse(cuentaCreada.Id);
                usuario.FechaDeModificacion = DateTime.Now;
                medihub.SaveChanges();
            }
        }

        // Devuelve la cuenta creada, o null si ya existía o no se pudo crear.
        private static ApplicationUser CrearCuenta(UserManager<ApplicationUser> usuarios, string correo,
            string nombre, string contrasena, string rol)
        {
            if (usuarios.FindByName(correo) != null) return null;

            var cuenta = new ApplicationUser { UserName = correo, Email = correo, NombreCompleto = nombre };
            if (!usuarios.Create(cuenta, contrasena).Succeeded) return null;

            usuarios.AddToRole(cuenta.Id, rol);
            return cuenta;
        }
    }
}
