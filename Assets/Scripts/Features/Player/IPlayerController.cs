using System;
using Features.Tanks;

namespace Features.Player
{
    public interface IPlayerController : IDisposable
    {
        void Bind(Tank tank);
    }
}