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
    // Enlace entre la cuenta de ASP.NET Identity y el registro de Usuario (Usuario.IdNetUser).
    public class UsuarioServiceVincularCuentaTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 11, 19, 9, 0, 0);
        private static readonly Guid IdCuenta = Guid.NewGuid();

        private readonly Mock<IUsuarioRepository> _repositorio = new Mock<IUsuarioRepository>();
        private readonly Mock<IProveedorRepository> _proveedores = new Mock<IProveedorRepository>();
        private readonly UsuarioService _servicio;

        public UsuarioServiceVincularCuentaTests()
        {
            _proveedores.Setup(p => p.ObtenerPorId(1)).Returns(new Proveedor { IdProveedor = 1 });
            _servicio = new UsuarioService(_repositorio.Object, _proveedores.Object, new UsuarioDtoValidator(),
                Mapeador.Crear(), new RelojFijo(Hoy), new Mock<IBitacoraService>().Object);
        }

        [Fact]
        public void VincularCuenta_IdentificacionNueva_CreaElUsuarioConElIdDeLaCuenta()
        {
            Usuario guardado = null;
            _repositorio.Setup(r => r.Agregar(It.IsAny<Usuario>())).Callback<Usuario>(u => guardado = u);

            var resultado = _servicio.VincularCuenta(UsuarioValido(), IdCuenta);

            resultado.Exito.Should().BeTrue();
            guardado.IdNetUser.Should().Be(IdCuenta);
            guardado.FechaDeRegistro.Should().Be(Hoy);
            guardado.Estado.Should().BeTrue();
        }

        [Fact]
        public void VincularCuenta_UsuarioExistenteSinCuenta_LoEnlazaSinDuplicarlo()
        {
            var existente = new Usuario
            {
                IdUsuario = 7,
                IdProveedor = 1,
                Identificacion = "112340567",
                CorreoElectronico = "MRodriguez@DrogueriaCentral.cr"
            };
            _repositorio.Setup(r => r.ObtenerPorIdentificacion("112340567")).Returns(existente);

            var resultado = _servicio.VincularCuenta(UsuarioValido(), IdCuenta);

            resultado.Exito.Should().BeTrue();
            existente.IdNetUser.Should().Be(IdCuenta);
            existente.FechaDeModificacion.Should().Be(Hoy);
            _repositorio.Verify(r => r.Actualizar(existente), Times.Once);
            _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void VincularCuenta_UsuarioQueYaTieneCuenta_NoLaSobrescribe()
        {
            var cuentaAnterior = Guid.NewGuid();
            var existente = new Usuario
            {
                IdUsuario = 7,
                IdProveedor = 1,
                IdNetUser = cuentaAnterior,
                Identificacion = "112340567",
                CorreoElectronico = "mrodriguez@drogueriacentral.cr"
            };
            _repositorio.Setup(r => r.ObtenerPorIdentificacion("112340567")).Returns(existente);

            var resultado = _servicio.VincularCuenta(UsuarioValido(), IdCuenta);

            resultado.Exito.Should().BeFalse();
            existente.IdNetUser.Should().Be(cuentaAnterior);
            _repositorio.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void VincularCuenta_IdentificacionDeOtroProveedor_NoEnlaza()
        {
            var existente = new Usuario
            {
                IdUsuario = 7,
                IdProveedor = 2,
                Identificacion = "112340567",
                CorreoElectronico = "mrodriguez@drogueriacentral.cr"
            };
            _repositorio.Setup(r => r.ObtenerPorIdentificacion("112340567")).Returns(existente);

            var resultado = _servicio.VincularCuenta(UsuarioValido(), IdCuenta);

            resultado.Exito.Should().BeFalse();
            existente.IdNetUser.Should().BeNull();
        }

        [Fact]
        public void VincularCuenta_CorreoDistinto_NoEnlaza()
        {
            var existente = new Usuario
            {
                IdUsuario = 7,
                IdProveedor = 1,
                Identificacion = "112340567",
                CorreoElectronico = "otro@drogueriacentral.cr"
            };
            _repositorio.Setup(r => r.ObtenerPorIdentificacion("112340567")).Returns(existente);

            var resultado = _servicio.VincularCuenta(UsuarioValido(), IdCuenta);

            resultado.Exito.Should().BeFalse();
            existente.IdNetUser.Should().BeNull();
        }

        [Fact]
        public void VincularCuenta_DatosInvalidos_DevuelveErroresDeValidacion()
        {
            var dto = UsuarioValido();
            dto.Identificacion = "abc";

            var resultado = _servicio.VincularCuenta(dto, IdCuenta);

            resultado.Errores.Should().Contain(e => e.Propiedad == "Identificacion");
            _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void ObtenerPorIdNetUser_CuentaEnlazada_DevuelveElUsuario()
        {
            _repositorio.Setup(r => r.ObtenerPorIdNetUser(IdCuenta)).Returns(new Usuario
            {
                IdUsuario = 7,
                IdProveedor = 1,
                IdNetUser = IdCuenta,
                Proveedor = new Proveedor { IdProveedor = 1, Nombre = "Droguería Central S.A." }
            });

            var usuario = _servicio.ObtenerPorIdNetUser(IdCuenta);

            usuario.IdUsuario.Should().Be(7);
            usuario.NombreProveedor.Should().Be("Droguería Central S.A.");
        }

        [Fact]
        public void ObtenerPorIdNetUser_CuentaSinUsuario_DevuelveNull()
        {
            _servicio.ObtenerPorIdNetUser(Guid.NewGuid()).Should().BeNull();
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
