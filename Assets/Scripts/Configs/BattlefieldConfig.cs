using System.Collections.Generic;
using Features.Tanks;
using Features.Tanks.Config;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Configs/BattlefieldConfig")]
    public class BattlefieldConfig : ScriptableObject
    {
        [Header("Map Setup")]
        [SerializeField] private Vector2 fieldSize= new(16,16);

        [Header("General")]
        [SerializeField] public int enemyAmount = 4;
        [SerializeField] public float moveCruiseSpeed = 2.5f;
        [SerializeField] public float respawnDelay = 1.0f;
        [SerializeField] public float spawnWallOffset = 2f;
        
        [Header("Prefabs and configs")]
        [SerializeField] public Tank playerTankPrefab;
        [SerializeField] public TankConfig playerConfig;
        
        [SerializeField] public Tank enemyTankPrefab;
        [SerializeField] public List<TankConfig> enemyConfigs = new();
        
        
        public Vector2 MapCenter => Vector2.zero;
        
        public Vector2 MapMin => new(-MapWidth * 0.5f, -MapHeight * 0.5f);
        public Vector2 MapMax => new(MapWidth * 0.5f, MapHeight * 0.5f);
        public float MapWidth => fieldSize.x;
        public float MapHeight => fieldSize.y;
    }
}