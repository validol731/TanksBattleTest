using System;
using Features.Combat.Config;
using Features.Combat.WeaponEntities;
using UnityEngine;

namespace Features.Combat
{
    public class WeaponFactory : IWeaponFactory
    {
        public IWeapon Build(WeaponConfig config, int levelIndex, bool isEnemy)
        {
            var lvl = config.levels[Mathf.Clamp(levelIndex, 0, config.levels.Count - 1)];
            switch (config.type)
            {
                case WeaponType.SimpleGun:
                    var gun = new SimpleGun();
                    gun.Setup(lvl, isEnemy);
                    return gun;

                default:
                    throw new NotImplementedException($"Weapon type {config.type} not implemented");
            }
        }
    }
}