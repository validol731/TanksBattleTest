using System;
using Features.Combat.Config;
using UnityEngine;

namespace Features.Tanks.Config
{
    [CreateAssetMenu(menuName = "Configs/TankConfig")]
    public class TankConfig : ScriptableObject
    {
        [SerializeField] public Sprite image;
        [SerializeField] public float maxForwardSpeed = 4f;
        [SerializeField] public float maxBackwardSpeed = 3f;
        [SerializeField] public float acceleration = 10f;
        [SerializeField] public float turnRadius = 2.0f;
        [SerializeField] public float omegaInPlace = 2.5f;
        [SerializeField] public float omegaMaxClamp = 2.5f;
        [SerializeField] public int maxHp = 1;
        [SerializeField] public WeaponSlot weapon = new();
        
        [Serializable]
        public struct WeaponSlot
        {
            public WeaponConfig config;
            [Min(0)] 
            public int levelIndex;
        }
    }
}