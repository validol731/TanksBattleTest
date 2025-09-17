using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Features.Tanks;
using Features.Tanks.Config;
using Configs;
using Features.AI;
using Features.Player;
using UniRx;
using VContainer.Unity;

namespace Features.Spawning
{
    public class BattlefieldSpawner : MonoBehaviour
    {
        [SerializeField] private int playerCornerIndex = 0;

        private IObjectResolver _resolver;
        private RespawnManager _respawnManager;
        private BattlefieldConfig _battlefieldConfig;

        [Inject]
        public void Construct(IObjectResolver resolver, RespawnManager respawnManager, BattlefieldConfig battlefieldConfig)
        {
            _resolver = resolver;
            _respawnManager = respawnManager;
            _battlefieldConfig = battlefieldConfig;
        }

        private void Start()
        {
            SpawnPlayer();
            SpawnEnemies();
        }

        private void SpawnPlayer()
        {
            TankConfig playerConfig = _battlefieldConfig.playerConfig;
            if (playerConfig == null)
            {
                Debug.LogError("[BattlefieldSpawner] PlayerConfig is null in BattlefieldConfig.");
                return;
            }

            Vector2 position = _respawnManager.CornerByIndex(playerCornerIndex);
            Quaternion rotation = Quaternion.identity;

            Tank player = Instantiate(_battlefieldConfig.playerTankPrefab, position, rotation);

            _resolver.InjectGameObject(player.gameObject);
            player.Initialize(playerConfig);
            var host = player.gameObject.AddComponent<PlayerControllerHost>();
            _resolver.Inject(host);

            player.Died.Subscribe(OnPlayerDie).AddTo(player);
        }
        
        private void OnPlayerDie(Tank player)
        {
            playerCornerIndex += 1;
            if (playerCornerIndex > 3)
            {
                playerCornerIndex = 0;
            }

            Vector2 nextCorner = _respawnManager.CornerByIndex(playerCornerIndex);
            _respawnManager.RespawnAfterDelay(player, nextCorner).Forget();
        }
        
        private void SpawnEnemies()
        {
            IReadOnlyList<TankConfig> configs = _battlefieldConfig.enemyConfigs;
            if (configs == null || configs.Count == 0)
            {
                Debug.LogError("[BattlefieldSpawner] EnemyConfigs list is empty in BattlefieldConfig.");
                return;
            }

            for (int i = 0; i < _battlefieldConfig.enemyAmount; i++)
            {
                int configIndex = Random.Range(0, configs.Count);
                TankConfig enemyConfig = configs[configIndex];

                Vector2 position = _respawnManager.RandomBorderPoint();
                Quaternion rotation = Quaternion.identity;

                Tank enemy = Instantiate(_battlefieldConfig.enemyTankPrefab, position, rotation);

                _resolver.InjectGameObject(enemy.gameObject);

                enemy.Initialize(enemyConfig);

                EnemyAIHost host = enemy.gameObject.AddComponent<EnemyAIHost>();
                _resolver.Inject(host);

                Vector2 center = _battlefieldConfig.MapCenter;
                Vector2 toCenter = (center - position).normalized;
                float headingDegrees = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
                enemy.transform.rotation = Quaternion.Euler(0f, 0f, headingDegrees);

                enemy.Died.Subscribe(OnEnemyDie).AddTo(enemy);
            }
        }

        
        private void OnEnemyDie(Tank enemy)
        {
            Vector2 newPosition = _respawnManager.RandomBorderPoint();
            _respawnManager.RespawnAfterDelay(enemy, newPosition).Forget();
        }
    }
}
