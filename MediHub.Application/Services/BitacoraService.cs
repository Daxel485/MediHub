using System;
using System.Diagnostics;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;
using Newtonsoft.Json;

namespace MediHub.Application.Services
{
    public class BitacoraService : IBitacoraService
    {
        private static readonly JsonSerializerSettings Formato = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly IBitacoraRepository _repositorio;
        private readonly IClock _clock;

        public BitacoraService(IBitacoraRepository repositorio, IClock clock)
        {
            _repositorio = repositorio;
            _clock = clock;
        }

        public void RegistrarEvento(string tabla, TipoEvento tipo, string descripcion,
            object datosAnteriores = null, object datosPosteriores = null)
        {
            Guardar(new BitacoraEvento
            {
                TablaDeEvento = tabla,
                TipoDeEvento = tipo.ToString(),
                FechaDeEvento = _clock.Ahora,
                DescripcionDeEvento = descripcion,
                DatosAnteriores = Serializar(datosAnteriores),
                DatosPosteriores = Serializar(datosPosteriores)
            });
        }

        public void RegistrarError(string tabla, Exception excepcion)
        {
            var raiz = excepcion.GetBaseException();
            Guardar(new BitacoraEvento
            {
                TablaDeEvento = tabla,
                TipoDeEvento = TipoEvento.Error.ToString(),
                FechaDeEvento = _clock.Ahora,
                DescripcionDeEvento = raiz.Message,
                StackTrace = excepcion.ToString()
            });
        }

        private void Guardar(BitacoraEvento evento)
        {
            // Si la bitácora falla no debe tumbar la operación principal.
            try
            {
                _repositorio.Registrar(evento);
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudo registrar el evento en la bitácora: {0}", ex);
            }
        }

        private static string Serializar(object datos)
        {
            return datos == null ? null : JsonConvert.SerializeObject(datos, Formato);
        }
    }
}
