using Features.Combat.Config;
using UnityEngine;

namespace Features.Combat
{
    public interface IWeapon
    {
        void Setup(WeaponConfig.WeaponLevel config, bool isEnemy);
        void Tick(float deltaTime);
        void TryFire(Vector2 pos, float headingRad);
    }
}