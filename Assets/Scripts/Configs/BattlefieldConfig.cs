using System;
using System.Collections.Generic;
using Features.AI.Config;
using Features.PowerUps.Config;
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
        [SerializeField] public Vector2 mapCenter = Vector2.zero;
        [SerializeField] public float spawnWallOffset = 2f;

        [Header("General")]
        
        [Header("Enemies by difficulty")]
        public List<EnemyPack> enemies = new();
        
        [Header("Player configs")]
        [SerializeField] public Tank playerTankPrefab;
        [SerializeField] public Tank enemyTankPrefab;
        [SerializeField] public TankConfig playerConfig;
        
        [Header("Power Ups")]
        [SerializeField] public PowerUpSpawnConfig powerUpSpawnConfig;
        
        
        public Vector2 MapMin => new(-MapWidth * 0.5f, -MapHeight * 0.5f);
        public Vector2 MapMax => new(MapWidth * 0.5f, MapHeight * 0.5f);
        public float MapWidth => fieldSize.x;
        public float MapHeight => fieldSize.y;
        
        [Serializable]
        public class EnemyPack
        {
            public int count = 1; 
            public AITankConfig tankConfig; 
        }
    }
}