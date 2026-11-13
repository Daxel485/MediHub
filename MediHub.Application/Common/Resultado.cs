using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace MediHub.Application.Common
{
    public class ErrorDeValidacion
    {
        public ErrorDeValidacion(string propiedad, string mensaje)
        {
            Propiedad = propiedad;
            Mensaje = mensaje;
        }

        public string Propiedad { get; }
        public string Mensaje { get; }
    }

    public class Resultado
    {
        private Resultado(bool exito, string mensaje, IReadOnlyList<ErrorDeValidacion> errores)
        {
            Exito = exito;
            Mensaje = mensaje;
            Errores = errores;
        }

        public bool Exito { get; }
        public string Mensaje { get; }
        public IReadOnlyList<ErrorDeValidacion> Errores { get; }
        public bool TieneErroresDeValidacion => Errores.Count > 0;

        public static Resultado Correcto(string mensaje)
        {
            return new Resultado(true, mensaje, new List<ErrorDeValidacion>());
        }

        public static Resultado Fallido(string mensaje)
        {
            return new Resultado(false, mensaje, new List<ErrorDeValidacion>());
        }

        public static Resultado Invalido(ValidationResult validacion)
        {
            var errores = validacion.Errors
                .Select(e => new ErrorDeValidacion(e.PropertyName, e.ErrorMessage))
                .ToList();
            return new Resultado(false, "Revise los datos ingresados.", errores);
        }
    }
}
