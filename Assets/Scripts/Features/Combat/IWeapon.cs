using UnityEngine;

namespace Features.Combat
{
    public interface IWeapon
    {
        void Setup(WeaponRuntimeConfig config);
        void Tick(float deltaTime);
        void TryFire(Vector2 pos, float headingRad);
    }
}