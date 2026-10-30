using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediHub.Application.Interfaces;
using MediHub.Application.Services;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Tests.Comun;
using Moq;
using Xunit;

namespace MediHub.Tests.Application
{
    public class ReporteMensualServiceTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 10, 29, 18, 30, 0);
        private static readonly DateTime InicioDelMes = new DateTime(2026, 10, 1);

        private readonly Mock<IReporteMensualRepository> _repositorio = new Mock<IReporteMensualRepository>();
        private readonly Mock<IProveedorRepository> _proveedores = new Mock<IProveedorRepository>();
        private readonly Mock<IConfiguracionProveedorRepository> _configuraciones = new Mock<IConfiguracionProveedorRepository>();
        private readonly ReporteMensualService _servicio;
        private List<ReporteMensual> _nuevos;

        public ReporteMensualServiceTests()
        {
            _proveedores.Setup(p => p.ObtenerTodos()).Returns(new List<Proveedor>
            {
                new Proveedor { IdProveedor = 1, Nombre = "Droguería Central" },
                new Proveedor { IdProveedor = 2, Nombre = "Distribuidora del Valle" }
            });
            _configuraciones.Setup(c => c.ObtenerTodas(null)).Returns(new List<ConfiguracionProveedor>
            {
                new ConfiguracionProveedor { IdProveedor = 1, Comision = 20, Estado = true }
            });
            _repositorio.Setup(r => r.ObtenerResumenDeOfertas(InicioDelMes, InicioDelMes.AddMonths(1)))
                .Returns(new List<ResumenMensualProveedor>
                {
                    new ResumenMensualProveedor { IdProveedor = 1, CantidadDeLicitaciones = 2, MontoTotalOfertado = 1000m, CantidadDeOfertas = 3 }
                });
            _repositorio.Setup(r => r.Guardar(It.IsAny<IEnumerable<ReporteMensual>>()))
                .Callback<IEnumerable<ReporteMensual>>(n => _nuevos = n.ToList());

            _servicio = new ReporteMensualService(_repositorio.Object, _proveedores.Object, _configuraciones.Object,
                Mapeador.Crear(), new RelojFijo(Hoy), new Mock<IBitacoraService>().Object);
        }

        [Fact]
        public void GenerarReportes_SinReportesDelMes_CreaUnoPorProveedorConLaComision()
        {
            SinReportesExistentes();

            var resultado = _servicio.GenerarReportes();

            resultado.Exito.Should().BeTrue();
            _nuevos.Should().HaveCount(2);
            var drogueria = _nuevos.Single(r => r.IdProveedor == 1);
            drogueria.CantidadDeLicitaciones.Should().Be(2);
            drogueria.CantidadDeOfertas.Should().Be(3);
            drogueria.MontoTotalOfertado.Should().Be(1000m);
            drogueria.MontoTotalComision.Should().Be(200m);
            drogueria.FechaDelReporte.Should().Be(Hoy);
        }

        [Fact]
        public void GenerarReportes_ProveedorSinConfiguracionNiOfertas_ReporteEnCero()
        {
            SinReportesExistentes();

            _servicio.GenerarReportes();

            var distribuidora = _nuevos.Single(r => r.IdProveedor == 2);
            distribuidora.CantidadDeOfertas.Should().Be(0);
            distribuidora.MontoTotalComision.Should().Be(0m);
        }

        [Fact]
        public void GenerarReportes_ReporteDelMesExistente_LoActualizaSinCrearOtro()
        {
            var existente = new ReporteMensual { IdReporte = 10, IdProveedor = 1, CantidadDeOfertas = 1, FechaDelReporte = InicioDelMes.AddDays(3) };
            _repositorio.Setup(r => r.ObtenerDelPeriodo(InicioDelMes, InicioDelMes.AddMonths(1)))
                .Returns(new List<ReporteMensual> { existente });

            _servicio.GenerarReportes();

            _nuevos.Should().ContainSingle(r => r.IdProveedor == 2);
            existente.CantidadDeOfertas.Should().Be(3);
            existente.MontoTotalComision.Should().Be(200m);
            existente.FechaDelReporte.Should().Be(Hoy);
        }

        private void SinReportesExistentes()
        {
            _repositorio.Setup(r => r.ObtenerDelPeriodo(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<ReporteMensual>());
        }
    }
}
