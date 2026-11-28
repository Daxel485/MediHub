using System;
using MediHub.Infrastructure.Identity;
using MediHub.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace MediHub.Web
{
    // Configuración de OWIN: contexto de Identity, administradores de cuentas y autenticación por cookie.
    public class Startup
    {
        public const string RutaLogin = "/Cuenta/Login";
        public const string RutaAccesoDenegado = "/Cuenta/AccesoDenegado";

        public void Configuration(IAppBuilder app)
        {
            ConfigurarAutenticacion(app);
        }

        private static void ConfigurarAutenticacion(IAppBuilder app)
        {
            // Una instancia de cada uno por solicitud.
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(RutaLogin),
                ExpireTimeSpan = TimeSpan.FromHours(8),
                SlidingExpiration = true,
                Provider = new CookieAuthenticationProvider
                {
                    // Revalida la cuenta cada 30 minutos (si cambia su contraseña o roles, se cierra la sesión).
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),

                    // [Authorize] responde 401 tanto si no hay sesión como si el rol no alcanza.
                    // Sin sesión se va al login; con sesión pero sin permiso, a "acceso denegado"
                    // (si no, el login redirigiría en bucle).
                    OnApplyRedirect = contexto =>
                    {
                        var autenticado = contexto.OwinContext.Authentication.User != null
                            && contexto.OwinContext.Authentication.User.Identity.IsAuthenticated;
                        var destino = autenticado
                            ? (contexto.Request.PathBase + new PathString(RutaAccesoDenegado)).Value
                            : contexto.RedirectUri;
                        contexto.Response.Redirect(destino);
                    }
                }
            });
        }
    }
}
