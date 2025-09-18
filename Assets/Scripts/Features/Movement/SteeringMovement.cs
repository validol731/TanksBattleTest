using Features.Tanks.Config;
using UnityEngine;

namespace Features.Movement
{
    public class SteeringMovement : IMovementController
    {
        private Rigidbody2D _rigidbody;
        private TankConfig _config;
        private float _headingRad;
        
        private const float TurnDeadzoneDeg      = 2.0f;
        private const float TurnHysteresisDeg    = 1.0f;
        private const float AngleForFullTurnDeg  = 45.0f;
                                                           
        private bool _wasTurning;
        private int  _stickyTurnSign;
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
                targetSpeed = _config.movementBaseConfig.maxForwardSpeed * forwardInput;
            }
            else
            {
                targetSpeed = _config.movementBaseConfig.maxBackwardSpeed * forwardInput;
            }

            float currentForwardSpeed = Vector2.Dot(_rigidbody.velocity, Direction(_headingRad));
            float desiredSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, _config.movementBaseConfig.acceleration * deltaTime);

            bool isInPlaceTurn = Mathf.Abs(forwardInput) < 0.0001f;
            if (isInPlaceTurn)
            {
                desiredSpeed = 0f;
            }

            float omegaDesired = turnInput * _config.movementBaseConfig.omegaMaxClamp;

            float omegaMax;
            if (isInPlaceTurn)
            {
                omegaMax = _config.movementBaseConfig.omegaInPlace;
            }
            else
            {
                float omegaMaxByRadius = Mathf.Abs(desiredSpeed) / Mathf.Max(0.01f, _config.movementBaseConfig.turnRadius);
                if (omegaMaxByRadius > _config.movementBaseConfig.omegaMaxClamp)
                {
                    omegaMax = _config.movementBaseConfig.omegaMaxClamp;
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

        public void MoveTowardsHeading(float targetHeadingRad, float cruiseSpeed, float dt)
        {
            float currentDeg = _headingRad * Mathf.Rad2Deg;
            float targetDeg = targetHeadingRad * Mathf.Rad2Deg;
            float deltaDeg = Mathf.DeltaAngle(currentDeg, targetDeg);
            float absDelta = Mathf.Abs(deltaDeg);

            bool inDeadzone = absDelta <= TurnDeadzoneDeg;
            bool exitDead = absDelta >= (TurnDeadzoneDeg + TurnHysteresisDeg);

            if (_wasTurning)
            {
                if (inDeadzone)
                {
                    _wasTurning = false;
                    _stickyTurnSign = 0;
                }
            }
            else
            {
                if (exitDead)
                {
                    _wasTurning = true;
                    _stickyTurnSign = deltaDeg > 0f ? 1 : -1;
                }
            }

            float turnInput = 0f;

            if (_wasTurning)
            {
                int signNow = deltaDeg > 0f ? 1 : -1;
                if (signNow != _stickyTurnSign && absDelta < (TurnDeadzoneDeg + TurnHysteresisDeg))
                {
                    deltaDeg = _stickyTurnSign * absDelta;
                }
                else
                {
                    _stickyTurnSign = signNow;
                }

                float normalized = deltaDeg / AngleForFullTurnDeg;
                if (normalized > 1f)
                {
                    normalized = 1f;
                }
                else if (normalized < -1f)
                {
                    normalized = -1f;
                }

                turnInput = normalized;
            }
            else
            {
                turnInput = 0f;
            }

            float forwardInput = 1f;
            if (_config.movementBaseConfig.maxForwardSpeed > 0.0001f)
            {
                float desiredInput = cruiseSpeed / _config.movementBaseConfig.maxForwardSpeed;
                if (desiredInput < 0f)
                {
                    desiredInput = 0f;
                }

                if (desiredInput > 1f)
                {
                    desiredInput = 1f;
                }

                forwardInput = desiredInput;
            }

            Move(forwardInput, turnInput, dt);
        }

        private static Vector2 Dir(float rad)
        {
            return new(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}