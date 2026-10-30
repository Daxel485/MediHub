using System;

namespace MediHub.Application.DTOs
{
    public class ProveedorDto
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }
    }
}
