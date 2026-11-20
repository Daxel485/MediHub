namespace MediHub.Domain.Enums
{
    // Etapas del ciclo de vida de una licitación. Se persiste como entero en la columna EstadoDelProceso.
    public enum EstadoLicitacion
    {
        Borrador = 1,
        Publicada = 2,
        EnEvaluacion = 3,
        Adjudicada = 4,
        Cerrada = 5
    }
}
