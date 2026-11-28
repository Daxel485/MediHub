using FluentAssertions;
using MediHub.Domain.Constants;
using Xunit;

namespace MediHub.Tests.Domain
{
    public class RolesTests
    {
        [Fact]
        public void Todos_IncluyeLosTresRolesDelSistema()
        {
            Roles.Todos.Should().BeEquivalentTo(new[] { "Administrador", "Comprador", "Proveedor" });
        }

        [Fact]
        public void DeRegistroPublico_NuncaIncluyeAlAdministrador()
        {
            Roles.DeRegistroPublico.Should().NotContain(Roles.Administrador);
            Roles.DeRegistroPublico.Should().BeEquivalentTo(new[] { Roles.Comprador, Roles.Proveedor });
        }

        [Fact]
        public void AdministradorOComprador_SeparaLosRolesConComa()
        {
            Roles.AdministradorOComprador.Should().Be("Administrador,Comprador");
        }
    }
}
