using System;

namespace MediHub.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdProveedor { get; set; }
        // Se enlaza con la cuenta de ASP.NET Identity cuando se integre el login.
        public Guid? IdNetUser { get; set; }
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Identificacion { get; set; }
        public string CorreoElectronico { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; }

        public virtual Proveedor Proveedor { get; set; }
    }
}
