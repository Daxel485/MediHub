using System.Data.Entity;
using MediHub.Infrastructure.Migrations;

namespace MediHub.Infrastructure.Data
{
    public static class BaseDeDatos
    {
        // Crea la base o aplica las migraciones pendientes la primera vez que se usa el contexto.
        public static void Inicializar()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MediHubContext, Configuration>());
        }
    }
}
