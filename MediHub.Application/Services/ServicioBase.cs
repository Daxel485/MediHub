using System;
using MediHub.Application.Common;
using MediHub.Application.Interfaces;

namespace MediHub.Application.Services
{
    public abstract class ServicioBase
    {
        protected const string MensajeErrorInesperado =
            "Ocurrió un error inesperado. El detalle quedó registrado en la bitácora.";

        protected ServicioBase(IBitacoraService bitacora)
        {
            Bitacora = bitacora;
        }

        protected IBitacoraService Bitacora { get; }

        // Ejecuta una operación de escritura y registra en la bitácora cualquier excepción no controlada.
        protected Resultado Ejecutar(string tabla, Func<Resultado> operacion)
        {
            try
            {
                return operacion();
            }
            catch (Exception ex)
            {
                Bitacora.RegistrarError(tabla, ex);
                return Resultado.Fallido(MensajeErrorInesperado);
            }
        }
    }
}
