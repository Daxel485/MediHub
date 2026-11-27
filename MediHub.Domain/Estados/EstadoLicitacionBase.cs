using System;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;

namespace MediHub.Domain.Estados
{
    // Patrón State: cada estado concreto decide qué transiciones permite y hacia qué estado lleva la licitación.
    // Por defecto toda transición está prohibida; cada estado sobrescribe solo las que admite.
    public abstract class EstadoLicitacionBase
    {
        private static readonly EstadoLicitacionBase Borrador = new EstadoBorrador();
        private static readonly EstadoLicitacionBase Publicada = new EstadoPublicada();
        private static readonly EstadoLicitacionBase EnEvaluacion = new EstadoEnEvaluacion();
        private static readonly EstadoLicitacionBase Adjudicada = new EstadoAdjudicada();
        private static readonly EstadoLicitacionBase Cerrada = new EstadoCerrada();

        public abstract EstadoLicitacion Tipo { get; }
        public abstract string Nombre { get; }

        public virtual bool PuedePublicar => false;
        public virtual bool PuedeIniciarEvaluacion => false;
        public virtual bool PuedeAdjudicar => false;
        public virtual bool PuedeCerrar => false;
        public virtual bool PermiteSincronizarOfertas => false;
        public virtual bool PermiteRecibirOfertas => false;

        public virtual void Publicar(Licitacion licitacion, DateTime ahora)
        {
            throw TransicionNoPermitida("publicar");
        }

        public virtual void IniciarEvaluacion(Licitacion licitacion, DateTime ahora)
        {
            throw TransicionNoPermitida("pasar a evaluación");
        }

        public virtual void Adjudicar(Licitacion licitacion, Oferta oferta, DateTime ahora)
        {
            throw TransicionNoPermitida("adjudicar");
        }

        public virtual void Cerrar(Licitacion licitacion, DateTime ahora)
        {
            throw TransicionNoPermitida("cerrar");
        }

        // Devuelve la instancia (sin estado propio, por eso se comparte) que corresponde al valor persistido.
        public static EstadoLicitacionBase Para(EstadoLicitacion tipo)
        {
            switch (tipo)
            {
                case EstadoLicitacion.Borrador: return Borrador;
                case EstadoLicitacion.Publicada: return Publicada;
                case EstadoLicitacion.EnEvaluacion: return EnEvaluacion;
                case EstadoLicitacion.Adjudicada: return Adjudicada;
                case EstadoLicitacion.Cerrada: return Cerrada;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Estado de licitación desconocido.");
            }
        }

        protected static void CambiarA(Licitacion licitacion, EstadoLicitacion nuevoEstado, DateTime ahora)
        {
            licitacion.EstadoDelProceso = nuevoEstado;
            licitacion.FechaDeModificacion = ahora;
        }

        private InvalidOperationException TransicionNoPermitida(string accion)
        {
            return new InvalidOperationException(
                "No se puede " + accion + " una licitación en estado " + Nombre + ".");
        }
    }
}
