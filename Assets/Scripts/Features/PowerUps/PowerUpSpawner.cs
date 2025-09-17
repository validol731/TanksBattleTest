// Features/PowerUps/PowerUpSpawner.cs
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Features.Tanks;
using Features.PowerUps.Config;
using Configs;
using UniRx;
using VContainer.Unity;

namespace Features.PowerUps
{
    public class PowerUpSpawner : MonoBehaviour
    {
        private BattlefieldConfig _battlefieldConfig;
        private PowerUpSpawnConfig _spawnConfig;
        private IObjectResolver _resolver;

        private CancellationTokenSource _token;

        private readonly Dictionary<PowerUpEntry, int> _alivePerEntry = new();
        private readonly List<PowerUpBase> _aliveInstances = new(16);

        [Inject]
        public void Construct(BattlefieldConfig battlefieldConfig, IObjectResolver resolver)
        {
            _battlefieldConfig = battlefieldConfig;
            _spawnConfig = battlefieldConfig.powerUpSpawnConfig;
            _resolver = resolver;
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        public void StartSpawning()
        {
            StopSpawning();

            if (_spawnConfig == null)
            {
                Debug.LogWarning("[PowerUpSpawner] PowerUpSpawnConfig is null on BattlefieldConfig.");
                return;
            }

            _token = new CancellationTokenSource();
            RunLoop(_token.Token).Forget();
        }

        public void StopSpawning()
        {
            if (_token != null)
            {
                _token.Cancel();
                _token.Dispose();
                _token = null;
            }
        }

        private async UniTaskVoid RunLoop(CancellationToken token)
        {
            if (_spawnConfig.firstSpawnDelay > 0f)
            {
                await UniTask.Delay((int)(_spawnConfig.firstSpawnDelay * 1000f), cancellationToken: token);
            }

            while (token.IsCancellationRequested == false)
            {
                if (_spawnConfig.items != null && _spawnConfig.items.Count > 0)
                {
                    int count = _spawnConfig.items.Count;
                    int startIndex = Random.Range(0, count);

                    for (int k = 0; k < count; k++)
                    {
                        int i = (startIndex + k) % count;
                        PowerUpEntry entry = _spawnConfig.items[i];
                        if (entry == null || entry.prefab == null)
                        {
                            continue;
                        }

                        _alivePerEntry.TryGetValue(entry, out var currentCount);
                        if (currentCount >= entry.maxOnField)
                        {
                            continue;
                        }

                        float chance = Mathf.Clamp01(entry.spawnAttemptChance);
                        if (Random.value > chance)
                        {
                            continue;
                        }

                        if (!ExistsEligibleTaker(entry))
                        {
                            continue;
                        }

                        if (!TryFindSpawnPosition(out var pos, _battlefieldConfig.spawnWallOffset, _spawnConfig.minDistanceFromPlayer, _spawnConfig.minDistanceBetweenPowerUps, 12))
                        {
                            continue;
                        }

                        SpawnOne(entry, pos);
                        break;
                    }
                }

                float wait = Random.Range(_spawnConfig.intervalRangeSeconds.x, _spawnConfig.intervalRangeSeconds.y);
                if (wait < 0.05f)
                {
                    wait = 0.05f;
                }

                await UniTask.Delay((int)(wait * 1000f), cancellationToken: token);
            }
        }

        private void SpawnOne(PowerUpEntry entry, Vector2 position)
        {
            PowerUpBase instance = Instantiate(entry.prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(instance.gameObject);
            instance.ConfigurePickupPermissions(entry.allowPlayerPickup, entry.allowEnemyPickup);
            instance.Collected.Subscribe(_ => OnCollected(entry, instance)).AddTo(instance);

            _aliveInstances.Add(instance);
            if (_alivePerEntry.ContainsKey(entry) == false)
            {
                _alivePerEntry[entry] = 0;
            }
            _alivePerEntry[entry] += 1;
        }

        private void OnCollected(PowerUpEntry entry, PowerUpBase instance)
        {
            _aliveInstances.Remove(instance);
            if (_alivePerEntry.ContainsKey(entry))
            {
                int v = _alivePerEntry[entry] - 1;
                if (v < 0)
                {
                    v = 0;
                }
                _alivePerEntry[entry] = v;
            }
        }

        private bool TryFindSpawnPosition(out Vector2 pos, float wallInset, float minDistFromPlayer, float minDistFromOthers, int attempts)
        {
            pos = Vector2.zero;

            for (int i = 0; i < attempts; i++)
            {
                Vector2 candidate = RandomPointInside(wallInset);

                if (!IsFarFromPlayer(candidate, minDistFromPlayer))
                {
                    continue;
                }

                if (!IsFarFromOtherPowerUps(candidate, minDistFromOthers))
                {
                    continue;
                }

                pos = candidate;
                return true;
            }

            return false;
        }

        private bool IsFarFromPlayer(Vector2 point, float minDist)
        {
            if (!_spawnConfig.avoidPlayer)
            {
                return true;
            }

            Transform player = FindPlayerTransform();
            if (player == null)
            {
                return true;
            }

            float d = Vector2.Distance(point, player.position);
            return d >= minDist;
        }

        private bool IsFarFromOtherPowerUps(Vector2 point, float minDist)
        {
            for (int i = 0; i < _aliveInstances.Count; i++)
            {
                PowerUpBase p = _aliveInstances[i];
                if (p == null)
                {
                    continue;
                }
                if (p.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                float d = Vector2.Distance(point, (Vector2)p.transform.position);
                if (d < minDist)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ExistsEligibleTaker(PowerUpEntry entry)
        {
            Tank[] tanks = FindObjectsOfType<Tank>(includeInactive: false);
            if (tanks == null || tanks.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < tanks.Length; i++)
            {
                Tank t = tanks[i];
                if (t == null)
                {
                    continue;
                }
                if (t.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                bool isPlayer = t.gameObject.CompareTag("Player");
                if (isPlayer && entry.allowPlayerPickup == false)
                {
                    continue;
                }
                if (!isPlayer && entry.allowEnemyPickup == false)
                {
                    continue;
                }

                PowerUpBase proto = entry.prefab;
                if (proto != null && proto.CanConsume(t))
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 RandomPointInside(float inset)
        {
            float halfMinSide = 0.5f * Mathf.Min(_battlefieldConfig.MapWidth, _battlefieldConfig.MapHeight);
            float safeInset = Mathf.Clamp(inset, 0f, halfMinSide - 0.01f);

            Vector2 min = _battlefieldConfig.MapMin + new Vector2(safeInset, safeInset);
            Vector2 max = _battlefieldConfig.MapMax - new Vector2(safeInset, safeInset);

            float x = Random.Range(min.x, max.x);
            float y = Random.Range(min.y, max.y);

            return new Vector2(x, y);
        }

        private Transform FindPlayerTransform()
        {
            GameObject go = GameObject.FindWithTag("Player");
            if (go == null)
            {
                return null;
            }
            return go.transform;
        }
    }
}
