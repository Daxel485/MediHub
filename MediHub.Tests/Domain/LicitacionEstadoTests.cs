using System;
using System.Collections.Generic;
using FluentAssertions;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using Xunit;

namespace MediHub.Tests.Domain
{
    public class LicitacionEstadoTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 11, 20, 10, 0, 0);

        [Fact]
        public void NuevaLicitacion_IniciaEnBorrador()
        {
            new Licitacion().EstadoDelProceso.Should().Be(EstadoLicitacion.Borrador);
        }

        [Fact]
        public void Publicar_Borrador_PasaAPublicadaYRegistraLaFecha()
        {
            var licitacion = Licitacion(EstadoLicitacion.Borrador);

            licitacion.Publicar(Hoy);

            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Publicada);
            licitacion.FechaDeModificacion.Should().Be(Hoy);
        }

        [Fact]
        public void Publicar_BorradorConFechaDeCierreVencida_LanzaExcepcion()
        {
            var licitacion = Licitacion(EstadoLicitacion.Borrador);
            licitacion.FechaDeCierre = Hoy.AddDays(-1);

            Action accion = () => licitacion.Publicar(Hoy);

            accion.Should().Throw<InvalidOperationException>();
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Borrador);
        }

        [Fact]
        public void IniciarEvaluacion_AntesDelCierre_LanzaExcepcion()
        {
            var licitacion = Licitacion(EstadoLicitacion.Publicada, new Oferta { IdOferta = 1 });

            Action accion = () => licitacion.IniciarEvaluacion(Hoy);

            accion.Should().Throw<InvalidOperationException>().WithMessage("*sigue abierta*");
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Publicada);
        }

        [Fact]
        public void IniciarEvaluacion_PublicadaSinOfertas_LanzaExcepcion()
        {
            var licitacion = Licitacion(EstadoLicitacion.Publicada);
            licitacion.FechaDeCierre = Hoy;

            Action accion = () => licitacion.IniciarEvaluacion(Hoy);

            accion.Should().Throw<InvalidOperationException>().WithMessage("*no tiene ofertas*");
        }

        [Fact]
        public void IniciarEvaluacion_PublicadaConOfertas_PasaAEnEvaluacion()
        {
            var licitacion = Licitacion(EstadoLicitacion.Publicada, new Oferta { IdOferta = 1 });
            licitacion.FechaDeCierre = Hoy;

            licitacion.IniciarEvaluacion(Hoy);

            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.EnEvaluacion);
        }

        [Fact]
        public void Adjudicar_EnEvaluacion_PasaAAdjudicada()
        {
            var licitacion = Licitacion(EstadoLicitacion.EnEvaluacion);

            licitacion.Adjudicar(Oferta(7, sincronizada: true), Hoy);

            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Adjudicada);
            licitacion.FechaDeModificacion.Should().Be(Hoy);
        }

        [Fact]
        public void Adjudicar_OfertaNoSincronizada_LanzaExcepcion()
        {
            var licitacion = Licitacion(EstadoLicitacion.EnEvaluacion);

            Action accion = () => licitacion.Adjudicar(Oferta(7, sincronizada: false), Hoy);

            accion.Should().Throw<InvalidOperationException>().WithMessage("*sincronizadas*");
            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.EnEvaluacion);
        }

        [Fact]
        public void Adjudicar_OfertaDeOtraLicitacion_LanzaExcepcion()
        {
            var licitacion = Licitacion(EstadoLicitacion.EnEvaluacion);
            var deOtra = Oferta(7, sincronizada: true);
            deOtra.IdLicitacion = 2;

            Action accion = () => licitacion.Adjudicar(deOtra, Hoy);

            accion.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData(EstadoLicitacion.Borrador)]
        [InlineData(EstadoLicitacion.Publicada)]
        [InlineData(EstadoLicitacion.EnEvaluacion)]
        [InlineData(EstadoLicitacion.Adjudicada)]
        public void Cerrar_DesdeCualquierEstadoNoFinal_PasaACerrada(EstadoLicitacion estado)
        {
            var licitacion = Licitacion(estado);

            licitacion.Cerrar(Hoy);

            licitacion.EstadoDelProceso.Should().Be(EstadoLicitacion.Cerrada);
        }

        [Fact]
        public void Cerrada_NoAdmiteNingunaTransicion()
        {
            var licitacion = Licitacion(EstadoLicitacion.Cerrada, new Oferta { IdOferta = 1, Sincronizada = true });

            ((Action)(() => licitacion.Publicar(Hoy))).Should().Throw<InvalidOperationException>();
            ((Action)(() => licitacion.IniciarEvaluacion(Hoy))).Should().Throw<InvalidOperationException>();
            ((Action)(() => licitacion.Adjudicar(Oferta(1, sincronizada: true), Hoy))).Should().Throw<InvalidOperationException>();
            ((Action)(() => licitacion.Cerrar(Hoy))).Should().Throw<InvalidOperationException>()
                .WithMessage("No se puede cerrar una licitación en estado Cerrada.");
        }

        [Theory]
        [InlineData(EstadoLicitacion.Publicada)]
        [InlineData(EstadoLicitacion.Adjudicada)]
        public void Publicar_FueraDeBorrador_LanzaExcepcion(EstadoLicitacion estado)
        {
            var licitacion = Licitacion(estado);

            Action accion = () => licitacion.Publicar(Hoy);

            accion.Should().Throw<InvalidOperationException>();
            licitacion.EstadoDelProceso.Should().Be(estado);
        }

        [Theory]
        [InlineData(EstadoLicitacion.Borrador, false)]
        [InlineData(EstadoLicitacion.Publicada, true)]
        [InlineData(EstadoLicitacion.EnEvaluacion, false)]
        [InlineData(EstadoLicitacion.Cerrada, false)]
        public void PermiteRecibirOfertas_SoloPublicada(EstadoLicitacion estado, bool esperado)
        {
            Licitacion(estado).EstadoActual.PermiteRecibirOfertas.Should().Be(esperado);
        }

        [Theory]
        [InlineData(EstadoLicitacion.Borrador, false)]
        [InlineData(EstadoLicitacion.Publicada, true)]
        [InlineData(EstadoLicitacion.EnEvaluacion, true)]
        [InlineData(EstadoLicitacion.Adjudicada, false)]
        [InlineData(EstadoLicitacion.Cerrada, false)]
        public void PermiteSincronizarOfertas_SoloMientrasSeRecibenOEvaluan(EstadoLicitacion estado, bool esperado)
        {
            Licitacion(estado).EstadoActual.PermiteSincronizarOfertas.Should().Be(esperado);
        }

        private static Oferta Oferta(int id, bool sincronizada)
        {
            return new Oferta { IdOferta = id, IdLicitacion = 1, Sincronizada = sincronizada };
        }

        private static Licitacion Licitacion(EstadoLicitacion estado, params Oferta[] ofertas)
        {
            return new Licitacion
            {
                IdLicitacion = 1,
                Titulo = "Compra de amoxicilina 500 mg",
                FechaDeCierre = Hoy.AddDays(15),
                Estado = true,
                EstadoDelProceso = estado,
                Ofertas = new List<Oferta>(ofertas)
            };
        }
    }
}
