using System;
using FluentAssertions;
using MediHub.Domain.Entities;
using Xunit;

namespace MediHub.Tests.Domain
{
    public class OfertaTests
    {
        [Fact]
        public void Sincronizar_OfertaPendiente_LaMarcaComoSincronizada()
        {
            var oferta = new Oferta { Sincronizada = false };

            oferta.Sincronizar();

            oferta.Sincronizada.Should().BeTrue();
        }

        [Fact]
        public void Sincronizar_OfertaYaSincronizada_LanzaExcepcion()
        {
            var oferta = new Oferta { Sincronizada = true };

            Action accion = () => oferta.Sincronizar();

            accion.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData(20, 1000, 200)]
        [InlineData(5, 1250000, 62500)]
        [InlineData(0, 5000, 0)]
        public void CalcularComision_AplicaElPorcentajeConfigurado(int comision, decimal monto, decimal esperado)
        {
            var configuracion = new ConfiguracionProveedor { Comision = comision, Estado = true };

            configuracion.CalcularComision(monto).Should().Be(esperado);
        }
    }
}
