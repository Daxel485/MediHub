using System;
using System.Collections.Generic;
using FluentAssertions;
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
    public class LicitacionServiceTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 11, 20, 10, 0, 0);

        private readonly Mock<ILicitacionRepository> _repositorio = new Mock<ILicitacionRepository>();
        private readonly Mock<IProveedorRepository> _proveedores = new Mock<IProveedorRepository>();
        private readonly Mock<IServicioCorreo> _correo = new Mock<IServicioCorreo>();
        private readonly Mock<IBitacoraService> _bitacora = new Mock<IBitacoraService>();
        private readonly LicitacionService _servicio;

        private readonly Proveedor _drogueria = Proveedor(1, "ventas@drogueriacentral.cr", true);
        private readonly Proveedor _distribuidora = Proveedor(2, "contacto@medvalle.cr", true);
        private readonly Proveedor _laboratorio = Proveedor(3, "licitaciones@andina.cr", false);

        public LicitacionServiceTests()
        {
            _proveedores.Setup(p => p.ObtenerTodos()).Returns(new[] { _drogueria, _distribuidora, _laboratorio });
            _servicio = new LicitacionService(_repositorio.Object, _proveedores.Object, new LicitacionDtoValidator(new RelojFijo(Hoy)),
                Mapeador.Crear(), new RelojFijo(Hoy), _bitacora.Object, Notificador.Crear(_correo.Object, _bitacora.Object));
        }

        [Fact]
        public void Publicar_Borrador_GuardaYNotificaSoloAProveedoresActivos()
        {
            var licitacion = Licitacion(EstadoLicitacion.Borrador);
            _repositorio.Setup(r => r.ObtenerParaActualizar(1)).Returns(licitacion);

            var resultado = _servicio.Publicar(1);

            resultado.Exito.Should().BeTrue();
            resultado.Mensaje.Should().Contain("2 notificaciones");
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Publicada);
            _repositorio.Verify(r => r.Actualizar(licitacion), Times.Once);
            _correo.Verify(c => c.Enviar(_drogueria.CorreoElectronico, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _correo.Verify(c => c.Enviar(_distribuidora.CorreoElectronico, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _correo.Verify(c => c.Enviar(_laboratorio.CorreoElectronico, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Publicar_TransicionNoPermitida_NoGuardaNiNotifica()
        {
            _repositorio.Setup(r => r.ObtenerParaActualizar(1)).Returns(Licitacion(EstadoLicitacion.Cerrada));

            var resultado = _servicio.Publicar(1);

            resultado.Exito.Should().BeFalse();
            resultado.Mensaje.Should().Contain("Cerrada");
            _repositorio.Verify(r => r.Actualizar(It.IsAny<Licitacion>()), Times.Never);
            _correo.Verify(c => c.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Publicar_LicitacionInexistente_DevuelveFallo()
        {
            _servicio.Publicar(404).Exito.Should().BeFalse();
        }

        [Fact]
        public void Cerrar_EnEvaluacion_NotificaRechazoATodosLosOferentes()
        {
            var licitacion = Licitacion(EstadoLicitacion.EnEvaluacion, Oferta(10, _drogueria), Oferta(11, _distribuidora));
            _repositorio.Setup(r => r.ObtenerParaActualizar(1)).Returns(licitacion);

            var resultado = _servicio.Cerrar(1);

            resultado.Exito.Should().BeTrue();
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Cerrada);
            _correo.Verify(c => c.Enviar(It.IsAny<string>(), It.Is<string>(a => a.StartsWith("Resultado")), It.IsAny<string>()),
                Times.Exactly(2));
        }

        [Fact]
        public void Cerrar_Adjudicada_NoEnviaCorreos()
        {
            var licitacion = Licitacion(EstadoLicitacion.Adjudicada, Oferta(10, _drogueria));
            _repositorio.Setup(r => r.ObtenerParaActualizar(1)).Returns(licitacion);

            _servicio.Cerrar(1).Exito.Should().BeTrue();

            _correo.Verify(c => c.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Publicar_FallaUnCorreo_LaTransicionSeConservaYSeRegistraElError()
        {
            var licitacion = Licitacion(EstadoLicitacion.Borrador);
            _repositorio.Setup(r => r.ObtenerParaActualizar(1)).Returns(licitacion);
            _correo.Setup(c => c.Enviar(_drogueria.CorreoElectronico, It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("SMTP no disponible"));

            var resultado = _servicio.Publicar(1);

            resultado.Exito.Should().BeTrue();
            resultado.Mensaje.Should().Contain("1 notificación").And.Contain("no se pudo enviar");
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Publicada);
            _repositorio.Verify(r => r.Actualizar(licitacion), Times.Once);
            _bitacora.Verify(b => b.RegistrarError("Notificacion", It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void Registrar_DatosValidos_QuedaEnBorrador()
        {
            Licitacion guardada = null;
            _repositorio.Setup(r => r.Agregar(It.IsAny<Licitacion>())).Callback<Licitacion>(l => guardada = l);

            var resultado = _servicio.Registrar(new MediHub.Application.DTOs.LicitacionDto
            {
                Titulo = "Reactivos de hematología",
                Descripcion = "Requiere cadena de frío.",
                FechaDeCierre = Hoy.AddDays(30),
                EstadoDelProceso = EstadoLicitacion.Adjudicada
            });

            resultado.Exito.Should().BeTrue();
            guardada.EstadoDelProceso.Should().Be(EstadoLicitacion.Borrador);
            _correo.Verify(c => c.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SincronizarOferta_LicitacionAdjudicada_NoLaSincroniza()
        {
            var oferta = new Oferta { IdOferta = 5, Licitacion = Licitacion(EstadoLicitacion.Adjudicada) };
            _repositorio.Setup(r => r.ObtenerOferta(5)).Returns(oferta);

            var resultado = _servicio.SincronizarOferta(5);

            resultado.Exito.Should().BeFalse();
            oferta.Sincronizada.Should().BeFalse();
            _repositorio.Verify(r => r.ActualizarOferta(It.IsAny<Oferta>()), Times.Never);
        }

        [Fact]
        public void ObtenerPorId_ExponeLasAccionesQuePermiteElEstado()
        {
            _repositorio.Setup(r => r.ObtenerPorId(1)).Returns(Licitacion(EstadoLicitacion.EnEvaluacion));

            var dto = _servicio.ObtenerPorId(1);

            dto.EstadoDelProcesoDescripcion.Should().Be("En evaluación");
            dto.PuedeAdjudicar.Should().BeTrue();
            dto.PuedeCerrar.Should().BeTrue();
            dto.PuedePublicar.Should().BeFalse();
            dto.PuedeIniciarEvaluacion.Should().BeFalse();
        }

        private static Licitacion Licitacion(EstadoLicitacion estado, params Oferta[] ofertas)
        {
            return new Licitacion
            {
                IdLicitacion = 1,
                Titulo = "Guantes de nitrilo talla M",
                FechaDeCierre = Hoy.AddDays(20),
                Estado = true,
                EstadoDelProceso = estado,
                Ofertas = new List<Oferta>(ofertas)
            };
        }

        private static Oferta Oferta(int id, Proveedor proveedor)
        {
            return new Oferta
            {
                IdOferta = id,
                IdLicitacion = 1,
                IdProveedor = proveedor.IdProveedor,
                Proveedor = proveedor,
                Monto = 870000m,
                Sincronizada = true
            };
        }

        private static Proveedor Proveedor(int id, string correo, bool activo)
        {
            return new Proveedor { IdProveedor = id, Nombre = "Proveedor " + id, CorreoElectronico = correo, Estado = activo };
        }
    }
}
