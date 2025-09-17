using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;
using Cysharp.Threading.Tasks;
using Configs;
using Features.Tanks;
using Features.Tanks.Config;
using Features.AI;
using Features.Player;
using VContainer.Unity;

namespace Features.Spawning
{
    public class BattlefieldSpawner : MonoBehaviour
    {
        private readonly float _minSpawnDistanceFromPlayer = 10f;
        private readonly float _minSpawnDistanceFromEnemies = 10f;

        private IObjectResolver _resolver;
        private BattlefieldConfig _config;

        private int _nextCornerIndex = 0;
        private bool _spawned;

        private Tank _player;
        private readonly List<Tank> _enemies = new List<Tank>(32);

        [Inject]
        public void Construct(IObjectResolver resolver, BattlefieldConfig config)
        {
            _resolver = resolver;
            _config = config;
        }

        public void SpawnAll()
        {
            if (_spawned)
            {
                return;
            }

            _spawned = true;
            _nextCornerIndex = 0;

            SpawnPlayer();
            SpawnEnemiesFromPacks();
        }

        private void SpawnPlayer()
        {
            TankConfig playerConfig = _config.playerConfig;
            if (playerConfig == null)
            {
                Debug.LogError("[BattlefieldSpawner] PlayerConfig is null.");
                return;
            }
            if (_config.playerTankPrefab == null)
            {
                Debug.LogError("[BattlefieldSpawner] playerTankPrefab is null.");
                return;
            }

            Vector2 position = CornerByIndex(_nextCornerIndex);
            Quaternion rotation = Quaternion.identity;

            Tank player = Instantiate(_config.playerTankPrefab, position, rotation);
            player.gameObject.tag = "Player";

            _resolver.InjectGameObject(player.gameObject);
            player.Initialize(playerConfig);

            PlayerControllerHost host = player.gameObject.AddComponent<PlayerControllerHost>();
            _resolver.Inject(host);

            player.Died.Subscribe(OnPlayerDie).AddTo(player);
            _player = player;
        }

        private void OnPlayerDie(Tank player)
        {
            Vector2 bestCorner = CornerByIndex(_nextCornerIndex);
            float bestScore = -1f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 corner = CornerByIndex(i);
                float distEnemies = MinDistanceToActiveEnemies(corner);
                float score = distEnemies;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCorner = corner;
                }
            }

            _nextCornerIndex += 1;
            if (_nextCornerIndex > 3)
            {
                _nextCornerIndex = 0;
            }

            RespawnAfterDelay(player, bestCorner).Forget();
        }

        private void SpawnEnemiesFromPacks()
        {
            if (_config.enemies == null || _config.enemies.Count == 0)
            {
                Debug.LogError("[BattlefieldSpawner] enemies list is empty in BattlefieldConfig.");
                return;
            }

            for (int p = 0; p < _config.enemies.Count; p++)
            {
                BattlefieldConfig.EnemyPack pack = _config.enemies[p];
                if (pack == null)
                {
                    continue;
                }
                if (pack.count <= 0)
                {
                    continue;
                }
                if (pack.tankConfig == null)
                {
                    Debug.LogWarning("[BattlefieldSpawner] EnemyPack has no TankConfig.");
                    continue;
                }

                for (int i = 0; i < pack.count; i++)
                {
                    Vector2 position = FindBorderPointSafe(_minSpawnDistanceFromPlayer, _minSpawnDistanceFromEnemies, 32);
                    Quaternion rotation = Quaternion.identity;

                    Tank prefab = _config.enemyTankPrefab;
                    if (prefab == null)
                    {
                        Debug.LogError("[BattlefieldSpawner] Enemy prefab is null.");
                        return;
                    }

                    Tank enemy = Instantiate(prefab, position, rotation);
                    _resolver.InjectGameObject(enemy.gameObject);
                    enemy.Initialize(pack.tankConfig);

                    Vector2 toCenter = (_config.MapCenter - position).normalized;
                    float headingDegrees = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
                    enemy.transform.rotation = Quaternion.Euler(0f, 0f, headingDegrees);

                    EnemyAIHost host = enemy.gameObject.AddComponent<EnemyAIHost>();
                    _resolver.Inject(host);
                    host.SetProfile(pack.tankConfig);

                    enemy.Died.Subscribe(OnEnemyDie).AddTo(enemy);
                    _enemies.Add(enemy);
                }
            }
        }

        private void OnEnemyDie(Tank enemy)
        {
            Vector2 newPosition = FindBorderPointSafe(_minSpawnDistanceFromPlayer, _minSpawnDistanceFromEnemies, 32);
            RespawnAfterDelay(enemy, newPosition).Forget();
        }

        public async UniTask RespawnAfterDelay(Tank tank, Vector2 position)
        {
            float delaySeconds = tank.RespawnDelay;

            tank.gameObject.SetActive(false);
            await UniTask.Delay((int)(delaySeconds * 1000f));

            tank.transform.position = position;
            tank.ResetForRespawn();
        }

        private Vector2 FindBorderPointSafe(float minDistFromPlayer, float minDistFromEnemies, int attempts)
        {
            Vector2 best = RandomBorderPoint();
            float bestScore = -1f;

            for (int i = 0; i < attempts; i++)
            {
                Vector2 p = RandomBorderPoint();

                bool farFromPlayer = IsFarFromPlayer(p, minDistFromPlayer);
                bool farFromEnemies = IsFarFromEnemies(p, minDistFromEnemies);

                if (farFromPlayer && farFromEnemies)
                {
                    return p;
                }

                float score = 0f;
                float dPlayer = DistanceToPlayer(p);
                float dEnemies = MinDistanceToActiveEnemies(p);
                score = dPlayer + dEnemies;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }

            return best;
        }

        private bool IsFarFromPlayer(Vector2 point, float minDist)
        {
            if (_player == null)
            {
                return true;
            }
            if (_player.gameObject.activeInHierarchy == false)
            {
                return true;
            }

            float d = Vector2.Distance(point, _player.transform.position);
            if (d >= minDist)
            {
                return true;
            }
            return false;
        }

        private bool IsFarFromEnemies(Vector2 point, float minDist)
        {
            float min = MinDistanceToActiveEnemies(point);
            if (min >= minDist)
            {
                return true;
            }
            return false;
        }

        private float DistanceToPlayer(Vector2 point)
        {
            if (_player == null)
            {
                return float.PositiveInfinity;
            }
            return Vector2.Distance(point, _player.transform.position);
        }

        private float MinDistanceToActiveEnemies(Vector2 point)
        {
            float min = float.PositiveInfinity;

            for (int i = 0; i < _enemies.Count; i++)
            {
                Tank e = _enemies[i];
                if (e == null)
                {
                    continue;
                }
                if (e.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                float d = Vector2.Distance(point, e.transform.position);
                if (d < min)
                {
                    min = d;
                }
            }

            if (float.IsPositiveInfinity(min))
            {
                return float.MaxValue;
            }
            return min;
        }

        private Vector2 RandomBorderPoint()
        {
            float halfMinSide = 0.5f * Mathf.Min(_config.MapWidth, _config.MapHeight);
            float inset = Mathf.Clamp(_config.spawnWallOffset, 0f, halfMinSide - 0.01f);

            Vector2 min = _config.MapMin + new Vector2(inset, inset);
            Vector2 max = _config.MapMax - new Vector2(inset, inset);

            int edgeIndex = Random.Range(0, 4);
            Vector2 point = Vector2.zero;

            if (edgeIndex == 0)
            {
                point = new Vector2(min.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 1)
            {
                point = new Vector2(max.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 2)
            {
                point = new Vector2(Random.Range(min.x, max.x), min.y);
            }
            else
            {
                point = new Vector2(Random.Range(min.x, max.x), max.y);
            }

            return point;
        }

        private Vector2 CornerByIndex(int index)
        {
            float halfMinSide = 0.5f * Mathf.Min(_config.MapWidth, _config.MapHeight);
            float inset = Mathf.Clamp(_config.spawnWallOffset, 0f, halfMinSide - 0.01f);

            Vector2 min = _config.MapMin + new Vector2(inset, inset);
            Vector2 max = _config.MapMax - new Vector2(inset, inset);

            if (index == 0)
            {
                return new Vector2(min.x, min.y);
            }
            else if (index == 1)
            {
                return new Vector2(max.x, min.y);
            }
            else if (index == 2)
            {
                return new Vector2(min.x, max.y);
            }
            else
            {
                return new Vector2(max.x, max.y);
            }
        }
    }
}
