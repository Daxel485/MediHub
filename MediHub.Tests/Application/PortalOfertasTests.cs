using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Application.Services;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Tests.Comun;
using Moq;
using Xunit;

namespace MediHub.Tests.Application
{
    public class PortalOfertasTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 11, 21, 9, 0, 0);
        private static readonly Guid IdNetUser = Guid.Parse("01192CF0-C811-4A26-9E79-023AEEC653A1");
        private readonly Mock<IPortalOfertasRepository> _repositorio = new Mock<IPortalOfertasRepository>();
        private readonly Mock<ITerminosLicitacion> _terminos = new Mock<ITerminosLicitacion>();
        private readonly Licitacion _licitacion = new Licitacion
        {
            IdLicitacion = 7, Estado = true, FechaDeCierre = Ahora.AddHours(1)
        };
        private readonly PortalOfertasService _servicio;

        public PortalOfertasTests()
        {
            _repositorio.Setup(r => r.ObtenerLicitacion(7)).Returns(_licitacion);
            _repositorio.Setup(r => r.ObtenerProveedorAutorizado(IdNetUser)).Returns(9);
            _repositorio.Setup(r => r.EjecutarEnLicitacion(7, It.IsAny<Func<Licitacion, Resultado>>()))
                .Returns((int id, Func<Licitacion, Resultado> operar) => operar(_licitacion));
            _servicio = new PortalOfertasService(_repositorio.Object, Mapeador.Crear(), new RelojFijo(Ahora),
                _terminos.Object, Mock.Of<IBitacoraService>());
        }

        [Fact]
        public void Comparar_AntesDelCierre_NoLeeLasOfertas()
        {
            var comparacion = _servicio.Comparar(7);
            comparacion.Ofertas.Should().BeEmpty();
            comparacion.PuedeAdjudicar.Should().BeFalse();
            _repositorio.Verify(r => r.ObtenerOfertas(7), Times.Never);
        }

        [Fact]
        public void Portal_SoloConsultaSiElProveedorAutenticadoYaOferto()
        {
            _repositorio.Setup(r => r.ObtenerVigente(7, Ahora)).Returns(_licitacion);
            _repositorio.Setup(r => r.TieneOferta(7, 9)).Returns(true);
            _servicio.ObtenerPortal(7, IdNetUser).TieneOfertaPropia.Should().BeTrue();
            _servicio.ObtenerPortal(7, Guid.Empty).Should().BeNull();
            _repositorio.Verify(r => r.TieneOferta(7, 9), Times.Once);
            _repositorio.Verify(r => r.ObtenerOfertas(7), Times.Never);
        }

        [Fact]
        public void Enviar_CalculaTotalDeDosItemsYUsaElProveedorAutenticado()
        {
            _terminos.Setup(t => t.ObtenerItems(_licitacion)).Returns(new[]
            {
                new ItemParaOfertarDto { IdItemLicitacion = 2, Cantidad = 3, Descripcion = "Ítem 2" },
                new ItemParaOfertarDto { IdItemLicitacion = 5, Cantidad = 8, Descripcion = "Ítem 5" }
            });
            Oferta guardada = null;
            _repositorio.Setup(r => r.AgregarOferta(It.IsAny<Oferta>())).Callback<Oferta>(oferta => guardada = oferta);

            var resultado = _servicio.EnviarOferta(7, IdNetUser, 4, null, new[]
            {
                Detalle(2, 230m),
                Detalle(5, 10.5m)
            });

            resultado.Exito.Should().BeTrue();
            guardada.IdProveedor.Should().Be(9);
            guardada.Monto.Should().Be(774m);
            guardada.Detalles.Should().HaveCount(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Enviar_VencidaORepetida_NoGuardaOtraOferta(bool vencida)
        {
            if (vencida) _licitacion.FechaDeCierre = Ahora;
            else _repositorio.Setup(r => r.TieneOferta(7, 9)).Returns(true);

            var resultado = _servicio.EnviarOferta(7, IdNetUser, 4, null, new[] { Detalle(2, 230m) });

            resultado.Exito.Should().BeFalse();
            _repositorio.Verify(r => r.AgregarOferta(It.IsAny<Oferta>()), Times.Never);
        }

        [Fact]
        public void Adjudicar_ExigeCierreYPertenencia_YGuardaElGanadorConSuEvento()
        {
            _repositorio.Setup(r => r.ObtenerOferta(7, 11)).Returns(Oferta(11, 9));
            _servicio.Adjudicar(7, 11, IdNetUser).Exito.Should().BeFalse();
            _licitacion.FechaDeCierre = Ahora;
            _servicio.Adjudicar(7, 99, IdNetUser).Exito.Should().BeFalse();
            _servicio.Adjudicar(7, 11, IdNetUser).Exito.Should().BeTrue();
            _repositorio.Setup(r => r.ObtenerOfertaGanadora(7)).Returns(11);
            _servicio.Adjudicar(7, 11, IdNetUser).Exito.Should().BeFalse();

            _repositorio.Verify(r => r.AgregarAdjudicacion(It.Is<Adjudicacion>(a =>
                a.IdLicitacion == 7 && a.IdOferta == 11 && a.IdComprador == IdNetUser &&
                a.FechaDeAdjudicacion == Ahora)), Times.Once);
            _repositorio.Verify(r => r.AgregarEvento(It.Is<BitacoraEvento>(e => e.TipoDeEvento == "Adjudicar" &&
                e.DatosPosteriores.Contains(IdNetUser.ToString()))), Times.Once);
        }

        [Fact]
        public void Comparacion_DevuelveSoloPuntajesPersistidosPorItem()
        {
            _licitacion.FechaDeCierre = Ahora;
            var oferta = Oferta(11, 9);
            oferta.Detalles.Add(new DetalleOferta { IdItemLicitacion = 2, Puntaje = 76.125m });
            oferta.Detalles.Add(new DetalleOferta { IdItemLicitacion = 5 });
            _repositorio.Setup(r => r.ObtenerOfertas(7)).Returns(new[] { oferta });

            var detalles = _servicio.Comparar(7).Ofertas.Single().Detalles;

            detalles.Single(d => d.IdItemLicitacion == 2).Puntaje.Should().Be(76.125m);
            detalles.Single(d => d.IdItemLicitacion == 5).Puntaje.Should().BeNull();
        }

        private static DetalleOfertaDto Detalle(int id, decimal precio) => new DetalleOfertaDto
        {
            IdItemLicitacion = id,
            PrecioUnitario = precio,
            FechaVencimientoLote = Ahora.AddMonths(4)
        };

        private static Oferta Oferta(int id, int proveedor) => new Oferta
        {
            IdOferta = id,
            IdProveedor = proveedor,
            Proveedor = new Proveedor { CorreoElectronico = $"proveedor{proveedor}@example.com" }
        };
    }
}
