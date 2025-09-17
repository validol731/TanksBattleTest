using Features.Tanks.Config;
using UnityEngine;

namespace Features.Movement
{
    public interface IMovementController
    {
        void Setup(Rigidbody2D rigidbody, TankConfig config);
        void Move(float forwardInput, float turnInput, float dt);
        void MoveTowardsHeading(float targetHeadingRad, float speed, float dt);
        float CurrentHeadingRad { get; }
    }
}