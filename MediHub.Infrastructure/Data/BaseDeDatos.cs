using System.Data.Entity;
using MediHub.Infrastructure.Migrations;

namespace MediHub.Infrastructure.Data
{
    public static class BaseDeDatos
    {
        public static void Inicializar()
        {
            // Migraciones aplicadas manualmente; se desactiva la validacion automatica del modelo.
            Database.SetInitializer<MediHubContext>(null);
        }
    }
}
