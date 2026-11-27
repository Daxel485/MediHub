using System;
using System.Security.Claims;
using System.Security.Principal;

namespace MediHub.Web.Extensions
{
    internal static class IdentidadExtensions
    {
        public static Guid ObtenerIdNetUser(this IPrincipal usuario)
        {
            var identidad = usuario.Identity as ClaimsIdentity;
            var identificador = identidad?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(identificador, out var id) ? id : Guid.Empty;
        }
    }
}
