using Features.Tanks.Config;
using UnityEngine;

namespace Features.Movement
{
    public class SteeringMovement : IMovementController
    {
        private Rigidbody2D _rigidbody;
        private TankConfig _config;
        private float _headingRad;

        public float CurrentHeadingRad => _headingRad;

        public void Setup(Rigidbody2D rigidbody, TankConfig config)
        {
            _rigidbody = rigidbody; 
            _config = config;
            _headingRad = _rigidbody.rotation * Mathf.Deg2Rad;
        }

        public void Move(float forwardInput, float turnInput, float deltaTime)
        {
            float targetSpeed;
            if (forwardInput >= 0f)
            {
                targetSpeed = _config.maxForwardSpeed * forwardInput;
            }
            else
            {
                targetSpeed = _config.maxBackwardSpeed * forwardInput;
            }

            float currentForwardSpeed = Vector2.Dot(_rigidbody.velocity, Direction(_headingRad));
            float desiredSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, _config.acceleration * deltaTime);

            bool isInPlaceTurn = Mathf.Abs(forwardInput) < 0.0001f;
            if (isInPlaceTurn)
            {
                desiredSpeed = 0f;
            }

            float omegaDesired = turnInput * _config.omegaMaxClamp;

            float omegaMax;
            if (isInPlaceTurn)
            {
                omegaMax = _config.omegaInPlace;
            }
            else
            {
                float omegaMaxByRadius = Mathf.Abs(desiredSpeed) / Mathf.Max(0.01f, _config.turnRadius);
                if (omegaMaxByRadius > _config.omegaMaxClamp)
                {
                    omegaMax = _config.omegaMaxClamp;
                }
                else
                {
                    omegaMax = omegaMaxByRadius;
                }
            }

            float angularVelocity = Mathf.Clamp(omegaDesired, -omegaMax, omegaMax);

            _headingRad += angularVelocity * deltaTime;

            _rigidbody.MoveRotation(_headingRad * Mathf.Rad2Deg);
            _rigidbody.MovePosition(_rigidbody.position + Direction(_headingRad) * desiredSpeed * deltaTime);
        }

        private static Vector2 Direction(float radians)
        {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public void MoveTowardsHeading(float targetHeadingRad, float speed, float dt)
        {
            float delta = Mathf.DeltaAngle(_headingRad * Mathf.Deg2Rad, targetHeadingRad) ;
            float omegaMax = Mathf.Abs(speed) / Mathf.Max(0.01f, _config.turnRadius);
            float omega = Mathf.Clamp(delta / dt, -omegaMax, omegaMax);
            omega = Mathf.Clamp(omega, -_config.omegaMaxClamp, _config.omegaMaxClamp);

            _headingRad += omega * dt;
            _rigidbody.MoveRotation(_headingRad * Mathf.Rad2Deg);
            _rigidbody.MovePosition(_rigidbody.position + Dir(_headingRad) * speed * dt);
        }

        private static Vector2 Dir(float rad)
        {
            return new(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}