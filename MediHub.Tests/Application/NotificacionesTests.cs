using System;
using FluentAssertions;
using MediHub.Application.Interfaces;
using MediHub.Application.Notificaciones;
using MediHub.Domain.Entities;
using Moq;
using Xunit;

namespace MediHub.Tests.Application
{
    public class NotificacionesTests
    {
        private readonly Mock<IServicioCorreo> _correo = new Mock<IServicioCorreo>();

        private static readonly Licitacion Licitacion = new Licitacion
        {
            Titulo = "Guantes de nitrilo talla M",
            Descripcion = "500 cajas de 100 unidades.",
            FechaDeCierre = new DateTime(2026, 12, 10)
        };

        private static readonly Proveedor Proveedor = new Proveedor
        {
            IdProveedor = 1,
            Nombre = "Droguería Central S.A.",
            CorreoElectronico = "ventas@drogueriacentral.cr"
        };

        [Fact]
        public void Publicacion_CreaYEnviaElCorreoDePublicacion()
        {
            var notificacion = new CreadorNotificacionPublicacion(_correo.Object).Enviar(Licitacion, Proveedor);

            notificacion.Should().BeOfType<NotificacionPublicacion>();
            notificacion.Asunto.Should().Contain(Licitacion.Titulo);
            notificacion.Cuerpo.Should().Contain("10/12/2026");
            _correo.Verify(c => c.Enviar("ventas@drogueriacentral.cr", notificacion.Asunto, notificacion.Cuerpo), Times.Once);
        }

        [Fact]
        public void Adjudicacion_IncluyeElMontoDeLaOfertaGanadora()
        {
            var oferta = new Oferta { IdOferta = 3, Monto = 870000m };

            var notificacion = new CreadorNotificacionAdjudicacion(_correo.Object).Enviar(Licitacion, Proveedor, oferta);

            notificacion.Should().BeOfType<NotificacionAdjudicacion>();
            notificacion.Tipo.Should().Be(TipoNotificacion.Adjudicacion);
            notificacion.Cuerpo.Should().Contain("adjudicada").And.Contain("870");
            _correo.Verify(c => c.Enviar(Proveedor.CorreoElectronico, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Adjudicacion_SinOferta_NoEnviaNada()
        {
            Action accion = () => new CreadorNotificacionAdjudicacion(_correo.Object).Enviar(Licitacion, Proveedor);

            accion.Should().Throw<ArgumentNullException>();
            _correo.Verify(c => c.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Rechazo_CreaElCorreoDeRechazo()
        {
            var notificacion = new CreadorNotificacionRechazo(_correo.Object).Enviar(Licitacion, Proveedor);

            notificacion.Should().BeOfType<NotificacionRechazo>();
            notificacion.Cuerpo.Should().Contain("no fue seleccionada");
            notificacion.Destinatario.Should().Be(Proveedor.CorreoElectronico);
        }
    }
}
