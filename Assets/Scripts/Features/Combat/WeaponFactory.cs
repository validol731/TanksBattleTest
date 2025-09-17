using System;
using Features.Combat.Config;
using Features.Combat.WeaponEntities;
using UnityEngine;

namespace Features.Combat
{
    public class WeaponFactory : IWeaponFactory
    {
        public IWeapon Build(WeaponConfig config, int levelIndex)
        {
            var lvl = config.levels[Mathf.Clamp(levelIndex, 0, config.levels.Count - 1)];
            var cfg = new WeaponRuntimeConfig
            {
                BulletSpeed = lvl.bulletSpeed,
                BulletLife = lvl.bulletLife,
                Cooldown = lvl.cooldown,
                ProjectilesPerShot = lvl.projectilesPerShot,
                SpreadDeg = lvl.spreadDeg,
                BulletPrefab = lvl.bulletPrefab,
                Damage = lvl.damage,
            };

            switch (config.type)
            {
                case WeaponType.SimpleGun:
                    var gun = new SimpleGun();
                    gun.Setup(cfg);
                    return gun;

                default:
                    throw new NotImplementedException($"Weapon type {config.type} not implemented");
            }
        }
    }
}