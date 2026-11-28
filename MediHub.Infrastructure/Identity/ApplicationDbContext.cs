using Microsoft.AspNet.Identity.EntityFramework;

namespace MediHub.Infrastructure.Identity
{
    // Contexto de Identity (AspNetUsers, AspNetRoles, ...). Comparte la base MediHubDb con MediHubContext,
    // pero tiene su propio historial de migraciones (ver IdentityConfiguration).
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("MediHubConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
