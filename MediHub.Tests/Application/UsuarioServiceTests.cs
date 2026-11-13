using System;
using FluentAssertions;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Application.Services;
using MediHub.Application.Validators;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Tests.Comun;
using Moq;
using Xunit;

namespace MediHub.Tests.Application
{
    public class UsuarioServiceTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 10, 29, 18, 30, 0);

        private readonly Mock<IUsuarioRepository> _repositorio = new Mock<IUsuarioRepository>();
        private readonly Mock<IProveedorRepository> _proveedores = new Mock<IProveedorRepository>();
        private readonly UsuarioService _servicio;

        public UsuarioServiceTests()
        {
            _proveedores.Setup(p => p.ObtenerPorId(1)).Returns(new Proveedor { IdProveedor = 1 });
            _servicio = new UsuarioService(_repositorio.Object, _proveedores.Object, new UsuarioDtoValidator(),
                Mapeador.Crear(), new RelojFijo(Hoy), new Mock<IBitacoraService>().Object);
        }

        [Fact]
        public void Registrar_IdentificacionRepetida_NoGuardaElUsuario()
        {
            _repositorio.Setup(r => r.ExisteIdentificacion("112340567", null)).Returns(true);

            var resultado = _servicio.Registrar(UsuarioValido());

            resultado.Exito.Should().BeFalse();
            _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void Registrar_DatosValidos_AsignaFechaYEstadoSinPedirlos()
        {
            Usuario guardado = null;
            _repositorio.Setup(r => r.Agregar(It.IsAny<Usuario>())).Callback<Usuario>(u => guardado = u);

            var resultado = _servicio.Registrar(UsuarioValido());

            resultado.Exito.Should().BeTrue();
            guardado.FechaDeRegistro.Should().Be(Hoy);
            guardado.Estado.Should().BeTrue();
            guardado.IdNetUser.Should().BeNull();
        }

        [Fact]
        public void Registrar_CorreoInvalido_DevuelveErrorDeValidacion()
        {
            var dto = UsuarioValido();
            dto.CorreoElectronico = "no-es-correo";

            var resultado = _servicio.Registrar(dto);

            resultado.Errores.Should().Contain(e => e.Propiedad == "CorreoElectronico");
        }

        [Fact]
        public void Editar_IdentificacionDeOtroUsuario_NoPermiteElCambio()
        {
            _repositorio.Setup(r => r.ObtenerPorId(5)).Returns(new Usuario { IdUsuario = 5, IdProveedor = 1 });
            _repositorio.Setup(r => r.ExisteIdentificacion("112340567", 5)).Returns(true);
            var dto = UsuarioValido();
            dto.IdUsuario = 5;

            var resultado = _servicio.Editar(dto);

            resultado.Exito.Should().BeFalse();
            _repositorio.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
        }

        private static UsuarioDto UsuarioValido()
        {
            return new UsuarioDto
            {
                IdProveedor = 1,
                Nombres = "María José",
                PrimerApellido = "Rodríguez",
                SegundoApellido = "Vargas",
                Identificacion = "112340567",
                CorreoElectronico = "mrodriguez@drogueriacentral.cr"
            };
        }
    }
}
