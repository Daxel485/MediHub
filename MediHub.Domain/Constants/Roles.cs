namespace MediHub.Domain.Constants
{
    // Nombres de los roles del sistema. Son constantes para poder usarlas en [Authorize(Roles = ...)].
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Comprador = "Comprador";
        public const string Proveedor = "Proveedor";

        // Combinaciones frecuentes (los roles se separan con coma en [Authorize]).
        public const string AdministradorOComprador = Administrador + "," + Comprador;

        public static readonly string[] Todos = { Administrador, Comprador, Proveedor };

        // Roles que una persona puede elegir al registrarse. El Administrador nunca se autoasigna.
        public static readonly string[] DeRegistroPublico = { Comprador, Proveedor };
    }
}
