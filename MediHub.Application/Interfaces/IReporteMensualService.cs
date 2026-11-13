using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IReporteMensualService
    {
        IReadOnlyList<ReporteMensualDto> ObtenerTodos();
        Resultado GenerarReportes();
    }
}
