using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
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

        private CancellationTokenSource _cts;

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

            _cts = new CancellationTokenSource();
            RunLoop(_cts.Token).Forget();
        }

        public void StopSpawning()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
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
                if (_spawnConfig.items != null)
                {
                    for (int i = 0; i < _spawnConfig.items.Count; i++)
                    {
                        PowerUpEntry entry = _spawnConfig.items[i];
                        if (entry == null)
                        {
                            continue;
                        }
                        if (entry.prefab == null)
                        {
                            continue;
                        }

                        int currentCount = 0;
                        if (_alivePerEntry.TryGetValue(entry, out int value))
                        {
                            currentCount = value;
                        }

                        if (currentCount >= entry.maxOnField)
                        {
                            continue;
                        }

                        float chance = Mathf.Clamp01(entry.spawnAttemptChance);
                        float roll = Random.value;
                        if (roll > chance)
                        {
                            continue;
                        }

                        TrySpawnOne(entry);
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

        private void TrySpawnOne(PowerUpEntry entry)
        {
            Vector2 position = RandomPointInside(_battlefieldConfig.spawnWallOffset);

            if (_spawnConfig.avoidPlayer)
            {
                Transform player = FindPlayerTransform();
                if (player != null)
                {
                    float distance = Vector2.Distance(position, player.position);
                    if (distance < _spawnConfig.minDistanceFromPlayer)
                    {
                        position = RandomPointInside(_battlefieldConfig.spawnWallOffset);
                    }
                }
            }

            PowerUpBase instance = Instantiate(entry.prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(instance.gameObject);

            instance.Collected.Subscribe(_ => OnCollected(entry, instance)).AddTo(instance);

            _aliveInstances.Add(instance);
            _alivePerEntry.TryAdd(entry, 0);
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
