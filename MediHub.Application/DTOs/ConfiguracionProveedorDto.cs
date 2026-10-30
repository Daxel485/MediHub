using System;
using MediHub.Domain.Enums;

namespace MediHub.Application.DTOs
{
    public class ConfiguracionProveedorDto
    {
        public int IdConfiguracion { get; set; }
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public TipoConfiguracion TipoConfiguracion { get; set; }
        public string TipoConfiguracionDescripcion { get; set; }
        public int Comision { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }
    }
}
