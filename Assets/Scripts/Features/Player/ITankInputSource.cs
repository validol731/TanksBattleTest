using System;
using UniRx;
using UnityEngine;

namespace Features.Player
{
    public interface ITankInputSource
    {
        IObservable<Vector2> Move { get; } // x = turn, y = forward
        IObservable<Unit> Fire { get; }
    }
}