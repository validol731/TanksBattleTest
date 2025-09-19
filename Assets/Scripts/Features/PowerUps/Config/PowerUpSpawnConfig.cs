using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.PowerUps.Config
{
    [CreateAssetMenu(menuName = "Configs/PowerUps/PowerUpSpawnConfig")]
    public class PowerUpSpawnConfig : ScriptableObject
    {
        [Header("Timing")] 
        public float firstSpawnDelay = 3f;
        public Vector2 intervalRangeSeconds = new Vector2(6f, 10f);

        [Header("Placement")] 
        public bool avoidPlayer = true;
        public float minDistanceFromPlayer = 4f;
        public float minDistanceBetweenPowerUps = 1.25f;

        [Header("Catalog (per-item rules)")] public List<PowerUpEntry> items = new();
    }

    [Serializable]
    public class PowerUpEntry
    {
        public string id;
        public PowerUpBase prefab;
        
        [Header("Limits")] 
        public int maxOnField = 3;

        [Header("Chance per tick")] 
        [Range(0f, 1f)]
        public float spawnAttemptChance = 1f;

        [Header("Pickup Permissions")] 
        public bool allowPlayerPickup = true;
        public bool allowEnemyPickup = true;
    }
}