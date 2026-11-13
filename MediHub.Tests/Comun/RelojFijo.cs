using System;
using MediHub.Application.Interfaces;

namespace MediHub.Tests.Comun
{
    internal sealed class RelojFijo : IClock
    {
        public RelojFijo(DateTime ahora)
        {
            Ahora = ahora;
        }

        public DateTime Ahora { get; }
    }
}
