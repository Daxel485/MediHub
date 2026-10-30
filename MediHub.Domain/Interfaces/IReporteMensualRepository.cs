using System;
using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IReporteMensualRepository
    {
        IReadOnlyList<ReporteMensual> ObtenerTodos();
        IReadOnlyList<ReporteMensual> ObtenerDelPeriodo(DateTime inicio, DateTime fin);
        IReadOnlyList<ResumenMensualProveedor> ObtenerResumenDeOfertas(DateTime inicio, DateTime fin);
        // Inserta los reportes nuevos y guarda los cambios de los existentes en una sola transacción.
        void Guardar(IEnumerable<ReporteMensual> nuevos);
    }
}
