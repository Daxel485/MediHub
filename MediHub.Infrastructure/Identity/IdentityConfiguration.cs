using System.Data.Entity.Migrations;

namespace MediHub.Infrastructure.Identity
{
    // Migraciones del contexto de Identity. Son automáticas porque las tablas AspNet* las define el
    // paquete de Identity y no el dominio; el ContextKey las separa de las migraciones de MediHubContext.
    internal sealed class IdentityConfiguration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public IdentityConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "MediHub.Infrastructure.Identity.IdentityConfiguration";
            MigrationsDirectory = @"Identity\Migrations";
        }
    }
}
