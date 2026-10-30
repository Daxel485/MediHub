using System;
using MediHub.Application.Interfaces;

namespace MediHub.Infrastructure.Servicios
{
    public sealed class SystemClock : IClock
    {
        public DateTime Ahora => DateTime.Now;
    }
}
