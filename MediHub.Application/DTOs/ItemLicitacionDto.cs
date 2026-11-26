namespace MediHub.Application.DTOs
{
    public class ItemLicitacionDto
    {
        public int IdItemLicitacion { get; set; }
        public int IdLicitacion { get; set; }
        public string Descripcion { get; set; }
        public decimal CantidadRequerida { get; set; }
        public string Unidad { get; set; }
        public int VidaUtilMeses { get; set; }
        public bool RequiereCadenaFrio { get; set; }
    }
}
