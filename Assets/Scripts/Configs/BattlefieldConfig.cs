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
        [SerializeField] public Vector2 fieldSize= new(16,16);

        [Header("General")]
        [SerializeField] public int enemyAmount = 4;
        
        [SerializeField] public float moveIntervalMin = 1.5f;
        [SerializeField] public float moveIntervalMax = 3.0f;
        [SerializeField] public float moveСruiseSpeed = 2.5f;
        [SerializeField] public float respawnDelay = 1.0f;
        
        [Header("Prefabs and configs")]
        [SerializeField] public Tank playerTankPrefab;
        [SerializeField] public TankConfig playerConfig;
        
        [SerializeField] public Tank enemyTankPrefab;
        [SerializeField] public List<TankConfig> enemyConfigs = new List<TankConfig>();
    }
}