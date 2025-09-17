using UnityEngine;
using Features.Movement;
using Configs;

namespace Features.AI
{
    public class WanderAI : IAIController
    {
        private IMovementController _movementController;
        private BattlefieldConfig _config;

        private float _timerSeconds;
        private float _targetHeadingRadians;
        private float _cruiseSpeed;

        public void Setup(IMovementController movementController, BattlefieldConfig config, Rigidbody2D rigidbody2D)
        {
            _movementController = movementController;
            _config = config;

            _cruiseSpeed = _config.moveСruiseSpeed;
            _targetHeadingRadians = _movementController.CurrentHeadingRad;
            _timerSeconds = 0f;
        }

        public void Tick(float deltaTime)
        {
            _timerSeconds -= deltaTime;
            if (_timerSeconds <= 0f)
            {
                _timerSeconds = Random.Range(_config.moveIntervalMin, _config.moveIntervalMax);
                _targetHeadingRadians = Random.Range(0f, Mathf.PI * 2f);
            }

            _movementController.MoveTowardsHeading(_targetHeadingRadians, _cruiseSpeed, deltaTime);
        }

        public void OnCollision()
        {
            float add = Random.Range(Mathf.Deg2Rad * 120f, Mathf.Deg2Rad * 180f);
            _targetHeadingRadians = _movementController.CurrentHeadingRad + add;
        }
    }
}