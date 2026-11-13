using System.Web.Mvc;
using MediHub.Application.Common;

namespace MediHub.Web.Extensions
{
    public static class ControllerExtensions
    {
        public const string ClaveExito = "MensajeExito";
        public const string ClaveError = "MensajeError";

        public static void AgregarErrores(this ModelStateDictionary modelState, Resultado resultado)
        {
            foreach (var error in resultado.Errores)
                modelState.AddModelError(error.Propiedad, error.Mensaje);
        }

        public static void MostrarResultado(this TempDataDictionary tempData, Resultado resultado)
        {
            tempData[resultado.Exito ? ClaveExito : ClaveError] = resultado.Mensaje;
        }
    }
}
