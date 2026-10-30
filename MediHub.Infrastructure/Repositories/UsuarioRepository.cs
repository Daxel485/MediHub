using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly MediHubContext _contexto;

        public UsuarioRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public IReadOnlyList<Usuario> ObtenerTodos()
        {
            return _contexto.Usuarios.AsNoTracking().Include(u => u.Proveedor)
                .OrderBy(u => u.PrimerApellido).ThenBy(u => u.Nombres)
                .ToList();
        }

        public Usuario ObtenerPorId(int idUsuario)
        {
            return _contexto.Usuarios.Include(u => u.Proveedor)
                .SingleOrDefault(u => u.IdUsuario == idUsuario);
        }

        public bool ExisteIdentificacion(string identificacion, int? excluirIdUsuario = null)
        {
            return _contexto.Usuarios.Any(u => u.Identificacion == identificacion
                && (!excluirIdUsuario.HasValue || u.IdUsuario != excluirIdUsuario.Value));
        }

        public void Agregar(Usuario usuario)
        {
            _contexto.Usuarios.Add(usuario);
            _contexto.SaveChanges();
        }

        public void Actualizar(Usuario usuario)
        {
            _contexto.SaveChanges();
        }
    }
}
