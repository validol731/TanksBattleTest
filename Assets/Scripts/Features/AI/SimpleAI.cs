using UnityEngine;
using Features.Movement;
using Configs;
using Features.Tanks;
using Features.PowerUps;
using Features.PowerUps.PowerUpsEntities;

namespace Features.AI
{
    public class SimpleAI : IAIController, IAIDebugData
    {
        private IMovementController _movementController;
        private BattlefieldConfig _config;
        private Rigidbody2D _rigidbody2D;
        private Tank _tank;

        private Transform _playerTransform;

        private float _brakeTimerSeconds;

        private float _repathTimerSeconds;
        private Vector2 _seekTarget;
        private float _targetHeadingRadians;
        private float _cruiseSpeed;

        private LayerMask _wallsMask;
        private LayerMask _powerUpMask;

        private readonly float _seekRepathInterval = 1f;
        private readonly float _seekJitterRadius = 16f;
        private readonly float _seekMinJitter = 6f;
        private readonly float _seekTargetMinDistance = 2.0f;
        private readonly float _arriveRadius = 1.0f;

        private readonly float _engageRadius = 8.0f;
        private readonly float _engageRepathInterval = 1f;
        private readonly float _stopAndAimDistance = 5.0f;
        private readonly float _fireAngleToleranceDeg = 6.0f;
        private readonly bool _useLineOfSight = true;

        private readonly float _brakeBeforeFireSeconds = 0.15f;
        private readonly float _boundsInset = 0.64f;
        
        private PowerUpBase _lootTarget;
        private float _lootScanTimer;
        private float _lootRepathTimer;
        private readonly float _lootScanInterval = 0.5f;
        private readonly float _lootRepathInterval = 0.3f;
        private readonly float _lootChaseRadius = 10.0f;
        private readonly float _lootAbortDistance = 12.0f;

        public bool HasPlayer => _playerTransform != null;

        public bool InEngage
        {
            get
            {
                if (_playerTransform == null)
                {
                    return false;
                }
                float distance = Vector2.Distance(_rigidbody2D.position, _playerTransform.position);
                return distance <= _engageRadius;
            }
        }

        public Vector2 SeekTarget => _seekTarget;
        public float TargetHeadingRadians => _targetHeadingRadians;

        public void Setup(IMovementController movementController, BattlefieldConfig config, Rigidbody2D rigidbody2D)
        {
            _movementController = movementController;
            _config = config;
            _rigidbody2D = rigidbody2D;
            _tank = rigidbody2D.GetComponent<Tank>();

            _cruiseSpeed = _config.moveCruiseSpeed;
            _targetHeadingRadians = _movementController.CurrentHeadingRad;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            int wallsLayer = LayerMask.NameToLayer("Walls");
            if (wallsLayer >= 0)
            {
                _wallsMask = 1 << wallsLayer;
            }
            else
            {
                _wallsMask = 0;
            }

            int powerupsLayer = LayerMask.NameToLayer("PowerUps");
            if (powerupsLayer >= 0)
            {
                _powerUpMask = 1 << powerupsLayer;
            }
            else
            {
                _powerUpMask = ~0;
            }

            _repathTimerSeconds = 0f;
            _seekTarget = _rigidbody2D.position;
            _brakeTimerSeconds = 0f;

            _lootScanTimer = 0f;
            _lootRepathTimer = 0f;
            _lootTarget = null;
        }

        public void Tick(float deltaTime)
        {
            EnsurePlayerRef();

            _lootScanTimer -= deltaTime;
            if (_lootScanTimer <= 0f)
            {
                UpdateLootTarget();
                _lootScanTimer = _lootScanInterval;
            }

            if (_lootTarget != null)
            {
                bool stillValid = ValidateLootTarget();
                if (stillValid)
                {
                    LootMove(deltaTime);
                    return;
                }
                _lootTarget = null;
            }

            float distanceToPlayer = float.PositiveInfinity;
            if (_playerTransform != null)
            {
                distanceToPlayer = Vector2.Distance(_rigidbody2D.position, _playerTransform.position);
            }

            bool inEngage = distanceToPlayer <= _engageRadius;

            _repathTimerSeconds -= deltaTime;
            if (_repathTimerSeconds <= 0f)
            {
                if (inEngage)
                {
                    _repathTimerSeconds = _engageRepathInterval;
                    UpdateEngageHeading();
                }
                else
                {
                    _repathTimerSeconds = _seekRepathInterval;
                    UpdateSeekTarget();
                }
            }

            if (inEngage)
            {
                EngageMoveAndFire(deltaTime, distanceToPlayer);
            }
            else
            {
                SeekMove(deltaTime);
            }
        }

        public void OnCollision()
        {
            float add = Random.Range(Mathf.Deg2Rad * 120f, Mathf.Deg2Rad * 170f);
            _targetHeadingRadians = _movementController.CurrentHeadingRad + add;
        }

        private void EnsurePlayerRef()
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }
        }

        private void UpdateLootTarget()
        {
            _lootTarget = FindBestPowerUp();
        }

        private bool ValidateLootTarget()
        {
            if (_lootTarget == null)
            {
                return false;
            }
            if (_lootTarget.gameObject.activeInHierarchy == false)
            {
                return false;
            }
            if (_lootTarget.CanBePickedBy(_tank) == false)
            {
                return false;
            }
            if (_lootTarget.CanConsume(_tank) == false)
            {
                return false;
            }

            float d = Vector2.Distance(_rigidbody2D.position, _lootTarget.transform.position);
            if (d > _lootAbortDistance)
            {
                return false;
            }

            return true;
        }

        private void LootMove(float deltaTime)
        {
            if (_lootTarget == null)
            {
                return;
            }

            _lootRepathTimer -= deltaTime;
            if (_lootRepathTimer <= 0f)
            {
                _lootRepathTimer = _lootRepathInterval;

                Vector2 pos = _rigidbody2D.position;
                Vector2 toLoot = (Vector2)_lootTarget.transform.position - pos;

                if (toLoot.sqrMagnitude > 0.0001f)
                {
                    _targetHeadingRadians = Mathf.Atan2(toLoot.y, toLoot.x);
                }

                _seekTarget = _lootTarget.transform.position;
            }

            _movementController.MoveTowardsHeading(_targetHeadingRadians, _cruiseSpeed, deltaTime);
        }

        private PowerUpBase FindBestPowerUp()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(_rigidbody2D.position, _lootChaseRadius, _powerUpMask);

            PowerUpBase best = null;
            int bestPriority = int.MinValue;
            float bestDist = float.PositiveInfinity;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D c = hits[i];
                if (c == null)
                {
                    continue;
                }

                PowerUpBase p = c.GetComponentInParent<PowerUpBase>();
                if (p == null)
                {
                    continue;
                }
                if (p.gameObject.activeInHierarchy == false)
                {
                    continue;
                }
                if (p.CanBePickedBy(_tank) == false)
                {
                    continue;
                }
                if (p.CanConsume(_tank) == false)
                {
                    continue;
                }

                int pri = p.GetAiPriority(_tank);
                float d = Vector2.Distance(_rigidbody2D.position, p.transform.position);

                bool betterByPriority = pri > bestPriority;
                bool samePriorityButCloser = pri == bestPriority && d < bestDist;

                if (betterByPriority || samePriorityButCloser)
                {
                    bestPriority = pri;
                    bestDist = d;
                    best = p;
                }
            }

            return best;
        }
        
        private Vector2 ClampInsideBoundsWithInset(Vector2 p)
        {
            Vector2 min = _config.MapMin + new Vector2(_boundsInset, _boundsInset);
            Vector2 max = _config.MapMax - new Vector2(_boundsInset, _boundsInset);

            float x = Mathf.Clamp(p.x, min.x, max.x);
            float y = Mathf.Clamp(p.y, min.y, max.y);

            return new Vector2(x, y);
        }

        private void UpdateSeekTarget()
        {
            Vector2 mapMin = _config.MapMin;
            Vector2 mapMax = _config.MapMax;

            Vector2 basePoint;
            if (_playerTransform != null)
            {
                basePoint = _playerTransform.position;
            }
            else
            {
                basePoint = (mapMin + mapMax) * 0.5f;
            }

            Vector2 currentPos = _rigidbody2D.position;
            Vector2 candidate = currentPos;
            bool found = false;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                float step = Random.Range(_seekMinJitter, _seekJitterRadius);
                Vector2 dir = Random.insideUnitCircle.normalized;

                candidate = basePoint + dir * step;
                candidate = ClampInsideBoundsWithInset(candidate);

                float distance = Vector2.Distance(candidate, currentPos);
                if (distance >= _seekTargetMinDistance)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Vector2 toBase = basePoint - currentPos;
                if (toBase.sqrMagnitude > 0.0001f)
                {
                    Vector2 dir = toBase.normalized;
                    candidate = currentPos + dir * _seekTargetMinDistance;
                }
                else
                {
                    candidate = currentPos + new Vector2(_seekTargetMinDistance, 0f);
                }

                candidate = ClampInsideBoundsWithInset(candidate);
            }

            _seekTarget = candidate;

            Vector2 toTarget = _seekTarget - currentPos;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                _targetHeadingRadians = Mathf.Atan2(toTarget.y, toTarget.x);
            }
        }

        private void SeekMove(float deltaTime)
        {
            float distanceToSeekTarget = Vector2.Distance(_rigidbody2D.position, _seekTarget);
            if (distanceToSeekTarget <= _arriveRadius)
            {
                _repathTimerSeconds = 0f;
            }

            _movementController.MoveTowardsHeading(_targetHeadingRadians, _cruiseSpeed, deltaTime);
        }

        private void UpdateEngageHeading()
        {
            if (_playerTransform == null)
            {
                return;
            }

            Vector2 toPlayer = (Vector2)_playerTransform.position - _rigidbody2D.position;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                _targetHeadingRadians = Mathf.Atan2(toPlayer.y, toPlayer.x);
            }
        }

        private void EngageMoveAndFire(float deltaTime, float distanceToPlayer)
        {
            if (_playerTransform == null)
            {
                return;
            }

            Vector2 toPlayer = (Vector2)_playerTransform.position - _rigidbody2D.position;

            float desiredDeg = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float currentDeg = _movementController.CurrentHeadingRad * Mathf.Rad2Deg;
            float deltaDeg = Mathf.DeltaAngle(currentDeg, desiredDeg);
            float absDelta = Mathf.Abs(deltaDeg);

            if (distanceToPlayer <= _stopAndAimDistance && absDelta > _fireAngleToleranceDeg)
            {
                float turnInput = 0f;
                if (deltaDeg > 0f)
                {
                    turnInput = 1f;
                }
                else if (deltaDeg < 0f)
                {
                    turnInput = -1f;
                }

                _movementController.Move(0f, turnInput, deltaTime);
                return;
            }

            if (absDelta <= _fireAngleToleranceDeg)
            {
                _brakeTimerSeconds -= deltaTime;
                if (_brakeTimerSeconds <= 0f)
                {
                    float speed = _rigidbody2D.velocity.magnitude;
                    if (speed > 0.05f)
                    {
                        _brakeTimerSeconds = _brakeBeforeFireSeconds;
                        _movementController.Move(0f, 0f, deltaTime);
                        return;
                    }

                    if (_useLineOfSight == false || HasLineOfSight(_rigidbody2D.position, _playerTransform.position))
                    {
                        _tank.TryFire();
                        _brakeTimerSeconds = _brakeBeforeFireSeconds;
                        _movementController.Move(0f, 0f, deltaTime);
                        return;
                    }
                }
                else
                {
                    _movementController.Move(0f, 0f, deltaTime);
                    return;
                }
            }
            _movementController.MoveTowardsHeading(_targetHeadingRadians, _cruiseSpeed, deltaTime);
        }

        private bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            if (_wallsMask == 0)
            {
                return true;
            }

            Vector2 direction = (to - from).normalized;
            float distance = Vector2.Distance(from, to);
            RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, _wallsMask);
            if (hit.collider == null)
            {
                return true;
            }
            return false;
        }
    }
}
