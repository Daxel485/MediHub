using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using MediHub.Domain.Constants;
using MediHub.Infrastructure.Identity;

namespace MediHub.Web.Extensions
{
    // Datos de la persona con sesión iniciada (User en controladores y vistas).
    public static class UsuarioActualExtensions
    {
        public static string ObtenerNombreCompleto(this IPrincipal usuario)
        {
            var identidad = usuario == null ? null : usuario.Identity as ClaimsIdentity;
            if (identidad == null) return string.Empty;

            var claim = identidad.FindFirst(ApplicationUser.ClaimNombreCompleto);
            return claim != null ? claim.Value : identidad.Name;
        }

        // Rol principal de la persona (Administrador, Comprador o Proveedor), o null si no tiene ninguno.
        public static string ObtenerRol(this IPrincipal usuario)
        {
            return usuario == null ? null : Roles.Todos.FirstOrDefault(usuario.IsInRole);
        }
    }
}
