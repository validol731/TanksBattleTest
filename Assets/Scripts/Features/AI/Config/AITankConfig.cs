using Features.Tanks.Config;
using UnityEngine;

namespace Features.AI.Config
{
    [CreateAssetMenu(menuName = "Configs/Tanks/AI Tank Config")]
    public class AITankConfig : TankConfig
    {
        public override bool IsEnemy => true;
        [Header("AI Difficulty (affects gameplay)")]
        public float fireAngleToleranceDeg = 8f;
        public float engageRadius = 8f;
        public float moveSpeedMultiplier = 1f;
        public float brakeBeforeFireSeconds = 0.15f;
        public float stopAndAimDistance = 5f;
        public float pursuitAbortDistance = 12f;

        [Header("PowerUps Behavior")]
        public float lootChaseRadius = 10f;
        public int lootPriorityBias = 0;

        [Header("Per-instance variance")]
        [Range(0f, 0.5f)] public float randomizePercent = 0.10f;
        [Range(0, 2)]    public int   lootPriorityJitter = 1;
        
        [Header("Score")]
        public int scoreOnKill = 100; 
    }
}