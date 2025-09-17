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

        [Header("Catalog (weighted)")]
        public List<PowerUpEntry> items = new List<PowerUpEntry>();
    }

    [Serializable]
    public class PowerUpEntry
    {
        public PowerUpBase prefab;
        public int maxOnField = 3;
        [Range(0f, 1f)] 
        public float spawnAttemptChance = 1f;
    }
}