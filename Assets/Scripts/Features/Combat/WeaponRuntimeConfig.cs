using UnityEngine;

namespace Features.Combat
{
    public struct WeaponRuntimeConfig
    {
        public float BulletSpeed;
        public float BulletLife;
        public float Cooldown;
        public float SpreadDeg;
        public int ProjectilesPerShot;
        public int Damage;
        public GameObject BulletPrefab;
    }
}