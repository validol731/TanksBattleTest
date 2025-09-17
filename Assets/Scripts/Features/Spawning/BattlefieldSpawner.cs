using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;
using Configs;
using Cysharp.Threading.Tasks;
using Features.Tanks;
using Features.Tanks.Config;
using Features.AI;
using Features.Player;
using VContainer.Unity;

namespace Features.Spawning
{
    public class BattlefieldSpawner : MonoBehaviour
    {
        [Header("Spawn Safety")]
        [SerializeField] private float minSpawnDistanceFromPlayer = 10f;
        [SerializeField] private float minSpawnDistanceFromEnemies = 10f;

        private IObjectResolver _resolver;
        private BattlefieldConfig _config;

        private int _nextCornerIndex = 0;
        private bool _spawned;

        private Tank _player;
        private readonly List<Tank> _enemies = new(16);

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
            SpawnEnemies();
        }

        private void SpawnPlayer()
        {
            TankConfig playerConfig = _config.playerConfig;
            if (playerConfig == null)
            {
                Debug.LogError("[BattlefieldSpawner] PlayerConfig is null in BattlefieldConfig.");
                return;
            }

            Vector2 position = CornerByIndex(_nextCornerIndex);
            Quaternion rotation = Quaternion.identity;

            Tank player = Instantiate(_config.playerTankPrefab, position, rotation);
            player.gameObject.tag = "Player";

            _resolver.InjectGameObject(player.gameObject);
            player.Initialize(playerConfig);

            var host = player.gameObject.AddComponent<PlayerControllerHost>();
            _resolver.Inject(host);

            player.Died.Subscribe(OnPlayerDie).AddTo(player);
            _player = player;
        }

        private void OnPlayerDie(Tank player)
        {
            Vector2 bestCorner = CornerByIndex(_nextCornerIndex);
            float bestCornerMinEnemyDist = -1f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 corner = CornerByIndex(i);
                float minDistToEnemy = MinDistanceToActiveEnemies(corner);

                if (minDistToEnemy >= minSpawnDistanceFromEnemies)
                {
                    bestCorner = corner;
                    break;
                }

                if (minDistToEnemy > bestCornerMinEnemyDist)
                {
                    bestCornerMinEnemyDist = minDistToEnemy;
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

        private void SpawnEnemies()
        {
            IReadOnlyList<TankConfig> configs = _config.enemyConfigs;
            if (configs == null || configs.Count == 0)
            {
                Debug.LogError("[BattlefieldSpawner] EnemyConfigs list is empty in BattlefieldConfig.");
                return;
            }

            for (int i = 0; i < _config.enemyAmount; i++)
            {
                int configIndex = Random.Range(0, configs.Count);
                TankConfig enemyConfig = configs[configIndex];

                Vector2 position = FindBorderPointFarFromPlayer(minSpawnDistanceFromPlayer, 24);
                Quaternion rotation = Quaternion.identity;

                Tank enemy = Instantiate(_config.enemyTankPrefab, position, rotation);
                _resolver.InjectGameObject(enemy.gameObject);
                enemy.Initialize(enemyConfig);

                Vector2 center = _config.MapCenter;
                Vector2 toCenter = (center - position).normalized;
                float headingDegrees = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
                enemy.transform.rotation = Quaternion.Euler(0f, 0f, headingDegrees);

                EnemyAIHost host = enemy.gameObject.AddComponent<EnemyAIHost>();
                _resolver.Inject(host);

                enemy.Died.Subscribe(OnEnemyDie).AddTo(enemy);

                _enemies.Add(enemy);
            }
        }

        private void OnEnemyDie(Tank enemy)
        {
            Vector2 newPosition = FindBorderPointFarFromPlayer(minSpawnDistanceFromPlayer, 24);
            RespawnAfterDelay(enemy, newPosition).Forget();
        }

        public async UniTask RespawnAfterDelay(Tank tank, Vector2 position)
        {
            float delaySeconds = _config.respawnDelay;

            tank.gameObject.SetActive(false);
            await UniTask.Delay((int)(delaySeconds * 1000f));

            tank.transform.position = position;
            tank.ResetForRespawn();
        }
        private Vector2 FindBorderPointFarFromPlayer(float minDist, int attempts)
        {
            Vector2 best = RandomBorderPoint();
            float bestDist = -1f;

            for (int i = 0; i < attempts; i++)
            {
                Vector2 p = RandomBorderPoint();

                if (IsFarFromPlayer(p, minDist))
                {
                    return p;
                }

                float d = DistanceToPlayer(p);
                if (d > bestDist)
                {
                    bestDist = d;
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

            if (edgeIndex == 0) // left
            {
                point = new Vector2(min.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 1) // right
            {
                point = new Vector2(max.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 2) // bottom
            {
                point = new Vector2(Random.Range(min.x, max.x), min.y);
            }
            else // top
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
                return new Vector2(min.x, min.y); // bottom-left
            }
            else if (index == 1)
            {
                return new Vector2(max.x, min.y); // bottom-right
            }
            else if (index == 2)
            {
                return new Vector2(min.x, max.y); // top-left
            }
            else
            {
                return new Vector2(max.x, max.y); // top-right
            }
        }
    }
}
