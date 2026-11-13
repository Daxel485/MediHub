using System;
using FluentAssertions;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Application.Services;
using MediHub.Application.Validators;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;
using MediHub.Tests.Comun;
using Moq;
using Xunit;

namespace MediHub.Tests.Application
{
    public class ConfiguracionProveedorServiceTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 10, 29, 18, 30, 0);

        private readonly Mock<IConfiguracionProveedorRepository> _repositorio = new Mock<IConfiguracionProveedorRepository>();
        private readonly Mock<IProveedorRepository> _proveedores = new Mock<IProveedorRepository>();
        private readonly Mock<IBitacoraService> _bitacora = new Mock<IBitacoraService>();
        private readonly ConfiguracionProveedorService _servicio;

        public ConfiguracionProveedorServiceTests()
        {
            _proveedores.Setup(p => p.ObtenerPorId(1)).Returns(new Proveedor { IdProveedor = 1, Nombre = "Droguería Central" });
            _servicio = new ConfiguracionProveedorService(_repositorio.Object, _proveedores.Object,
                new ConfiguracionProveedorDtoValidator(), Mapeador.Crear(), new RelojFijo(Hoy), _bitacora.Object);
        }

        [Fact]
        public void Registrar_ProveedorSinConfiguracion_GuardaConFechaYEstadoActivo()
        {
            ConfiguracionProveedor guardada = null;
            _repositorio.Setup(r => r.Agregar(It.IsAny<ConfiguracionProveedor>()))
                .Callback<ConfiguracionProveedor>(c => guardada = c);

            var resultado = _servicio.Registrar(Dto(TipoConfiguracion.Ambas, 10));

            resultado.Exito.Should().BeTrue();
            guardada.Should().NotBeNull();
            guardada.FechaDeRegistro.Should().Be(Hoy);
            guardada.FechaDeModificacion.Should().BeNull();
            guardada.Estado.Should().BeTrue();
            _bitacora.Verify(b => b.RegistrarEvento("ConfiguracionProveedor", TipoEvento.Registrar,
                It.IsAny<string>(), null, It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void Registrar_ProveedorConConfiguracionExistente_NoPermiteOtra()
        {
            _repositorio.Setup(r => r.ExisteParaProveedor(1)).Returns(true);

            var resultado = _servicio.Registrar(Dto(TipoConfiguracion.Plataforma, 5));

            resultado.Exito.Should().BeFalse();
            resultado.TieneErroresDeValidacion.Should().BeFalse();
            _repositorio.Verify(r => r.Agregar(It.IsAny<ConfiguracionProveedor>()), Times.Never);
        }

        [Fact]
        public void Registrar_ComisionFueraDeRango_DevuelveErrorDeValidacion()
        {
            var resultado = _servicio.Registrar(Dto(TipoConfiguracion.Plataforma, 150));

            resultado.TieneErroresDeValidacion.Should().BeTrue();
            resultado.Errores.Should().Contain(e => e.Propiedad == "Comision");
        }

        [Fact]
        public void Editar_ActualizaSoloCamposPermitidosYFechaDeModificacion()
        {
            var existente = new ConfiguracionProveedor
            {
                IdConfiguracion = 7, IdProveedor = 1, TipoConfiguracion = TipoConfiguracion.Plataforma,
                Comision = 5, Estado = true, FechaDeRegistro = Hoy.AddMonths(-1)
            };
            _repositorio.Setup(r => r.ObtenerPorId(7)).Returns(existente);

            var dto = Dto(TipoConfiguracion.Externa, 12);
            dto.IdConfiguracion = 7;
            dto.IdProveedor = 99;
            dto.Estado = false;
            var resultado = _servicio.Editar(dto);

            resultado.Exito.Should().BeTrue();
            existente.IdProveedor.Should().Be(1);
            existente.TipoConfiguracion.Should().Be(TipoConfiguracion.Externa);
            existente.Comision.Should().Be(12);
            existente.Estado.Should().BeFalse();
            existente.FechaDeRegistro.Should().Be(Hoy.AddMonths(-1));
            existente.FechaDeModificacion.Should().Be(Hoy);
            _repositorio.Verify(r => r.Actualizar(existente), Times.Once);
        }

        [Fact]
        public void Registrar_ErrorDelRepositorio_SeRegistraEnLaBitacora()
        {
            var falla = new InvalidOperationException("Sin conexión");
            _repositorio.Setup(r => r.Agregar(It.IsAny<ConfiguracionProveedor>())).Throws(falla);

            var resultado = _servicio.Registrar(Dto(TipoConfiguracion.Ambas, 10));

            resultado.Exito.Should().BeFalse();
            _bitacora.Verify(b => b.RegistrarError("ConfiguracionProveedor", falla), Times.Once);
        }

        private static ConfiguracionProveedorDto Dto(TipoConfiguracion tipo, int comision)
        {
            return new ConfiguracionProveedorDto { IdProveedor = 1, TipoConfiguracion = tipo, Comision = comision };
        }
    }
}
