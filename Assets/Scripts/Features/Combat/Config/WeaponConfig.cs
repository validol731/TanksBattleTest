using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Combat.Config
{
    public enum WeaponType
    {
        SimpleGun = 0,
    }
    [CreateAssetMenu(menuName = "Configs/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField] public string id;
        [SerializeField] public WeaponType type = WeaponType.SimpleGun;
        [SerializeField] public List<WeaponLevel> levels = new();
        
        [Serializable]
        public struct WeaponLevel
        {
            [Header("Ballistics")]
            public float bulletSpeed;
            public float bulletLife;

            [Header("Firing")]
            public float cooldown;
            public int projectilesPerShot;
            public float spreadDeg;

            [Header("Assets")]
            public GameObject playerBulletPrefab;
            public GameObject enemyBulletPrefab;

            [Header("Damage")]
            public int damage;
        }
    }
}