namespace MediHub.Domain.Entities
{
    // Totales de las ofertas de un proveedor en un mes. No se persiste.
    public class ResumenMensualProveedor
    {
        public int IdProveedor { get; set; }
        public int CantidadDeLicitaciones { get; set; }
        public decimal MontoTotalOfertado { get; set; }
        public int CantidadDeOfertas { get; set; }
    }
}
