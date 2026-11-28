using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MediHub.Infrastructure.Identity
{
    // Cuenta de acceso de ASP.NET Identity. Su Id (un Guid en texto) se guarda en Usuario.IdNetUser.
    public class ApplicationUser : IdentityUser
    {
        public const string ClaimNombreCompleto = "NombreCompleto";

        public string NombreCompleto { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var identidad = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            identidad.AddClaim(new Claim(ClaimNombreCompleto, string.IsNullOrWhiteSpace(NombreCompleto) ? UserName : NombreCompleto));
            return identidad;
        }
    }
}
