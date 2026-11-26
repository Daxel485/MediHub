using System;
using System.Collections.Generic;
using AutoMapper;
using FluentValidation;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;

namespace MediHub.Application.Services
{
    public class LicitacionService : ServicioBase, ILicitacionService
    {
        private const string TablaLicitacion = "Licitacion";
        private const string TablaOferta = "Oferta";
        private const string TablaItem = "ItemLicitacion";

        private readonly ILicitacionRepository _repositorio;
        private readonly IValidator<LicitacionDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public LicitacionService(ILicitacionRepository repositorio, IValidator<LicitacionDto> validador,
            IMapper mapper, IClock clock, IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<LicitacionDto> ObtenerTodas()
        {
            return _mapper.Map<List<LicitacionDto>>(_repositorio.ObtenerTodas());
        }

        public LicitacionDto ObtenerPorId(int idLicitacion)
        {
            var licitacion = _repositorio.ObtenerPorId(idLicitacion);
            return licitacion == null ? null : _mapper.Map<LicitacionDto>(licitacion);
        }

        public Resultado Registrar(LicitacionDto dto)
        {
            return Ejecutar(TablaLicitacion, () =>
            {
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_repositorio.ExisteTitulo(dto.Titulo))
                    return Resultado.Fallido("Ya existe una licitacion con el titulo \"" + dto.Titulo + "\".");

                var licitacion = _mapper.Map<Licitacion>(dto);
                licitacion.FechaDeRegistro = _clock.Ahora;
                licitacion.Estado = true;
                _repositorio.Agregar(licitacion);

                Bitacora.RegistrarEvento(TablaLicitacion, TipoEvento.Registrar,
                    "Se registro la licitacion " + licitacion.Titulo + ".",
                    datosPosteriores: new
                    {
                        licitacion.IdLicitacion,
                        licitacion.Titulo,
                        licitacion.Descripcion,
                        licitacion.FechaDeCierre,
                        licitacion.FechaDeRegistro,
                        licitacion.Estado
                    });
                return Resultado.Correcto("Licitacion registrada correctamente.");
            });
        }

        public Resultado ActualizarTerminos(int idLicitacion, string rutaPdf)
        {
            return Ejecutar(TablaLicitacion, () =>
            {
                var licitacion = _repositorio.ObtenerPorId(idLicitacion);
                if (licitacion == null) return Resultado.Fallido("La licitacion no existe.");

                // Puente para Persona 3: solo se permite si la licitacion esta activa.
                if (!licitacion.Estado)
                    return Resultado.Fallido("No se puede modificar una licitacion cerrada.");

                licitacion.RutaTerminos = rutaPdf;
                _repositorio.Actualizar(licitacion);
                return Resultado.Correcto("Terminos de referencia actualizados.");
            });
        }

        public Resultado AgregarItem(ItemLicitacionDto dto)
        {
            return Ejecutar(TablaItem, () =>
            {
                var licitacion = _repositorio.ObtenerPorId(dto.IdLicitacion);
                if (licitacion == null) return Resultado.Fallido("La licitacion no existe.");

                if (!licitacion.Estado)
                    return Resultado.Fallido("No se pueden agregar items a una licitacion cerrada.");

                var item = _mapper.Map<ItemLicitacion>(dto);
                item.IdItemLicitacion = 0;
                _repositorio.AgregarItem(item);

                Bitacora.RegistrarEvento(TablaItem, TipoEvento.Registrar,
                    "Se agrego el item \"" + item.Descripcion + "\" a la licitacion " + dto.IdLicitacion + ".",
                    datosPosteriores: new { item.IdItemLicitacion, item.Descripcion, item.CantidadRequerida });
                return Resultado.Correcto("Item agregado correctamente.");
            });
        }

        public Resultado EliminarItem(int idItem)
        {
            return Ejecutar(TablaItem, () =>
            {
                var item = _repositorio.ObtenerItem(idItem);
                if (item == null) return Resultado.Fallido("El item no existe.");

                var licitacion = _repositorio.ObtenerPorId(item.IdLicitacion);
                if (licitacion != null && !licitacion.Estado)
                    return Resultado.Fallido("No se pueden eliminar items de una licitacion cerrada.");

                Bitacora.RegistrarEvento(TablaItem, TipoEvento.Eliminar,
                    "Se elimino el item \"" + item.Descripcion + "\" (id=" + idItem + ").",
                    datosAnteriores: new { item.IdItemLicitacion, item.Descripcion });
                _repositorio.EliminarItem(item);
                return Resultado.Correcto("Item eliminado.");
            });
        }

        public IReadOnlyList<OfertaDto> ObtenerOfertas(int idLicitacion)
        {
            return _mapper.Map<List<OfertaDto>>(_repositorio.ObtenerOfertas(idLicitacion));
        }

        public Resultado SincronizarOferta(int idOferta)
        {
            return Ejecutar(TablaOferta, () =>
            {
                var oferta = _repositorio.ObtenerOferta(idOferta);
                if (oferta == null)
                    return Resultado.Fallido("La oferta no existe.");

                try
                {
                    oferta.Sincronizar();
                }
                catch (InvalidOperationException ex)
                {
                    return Resultado.Fallido(ex.Message);
                }

                _repositorio.ActualizarOferta(oferta);
                Bitacora.RegistrarEvento(TablaOferta, TipoEvento.Editar,
                    "Se sincronizo la oferta " + oferta.IdOferta + ".",
                    new { oferta.IdOferta, Sincronizada = false },
                    new { oferta.IdOferta, Sincronizada = true });
                return Resultado.Correcto("Oferta sincronizada correctamente.");
            });
        }
    }
}
