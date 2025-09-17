using Features.Combat.Config;

namespace Features.Combat
{
    public interface IWeaponFactory
    {
        IWeapon Build(WeaponConfig config, int levelIndex);
    }
}