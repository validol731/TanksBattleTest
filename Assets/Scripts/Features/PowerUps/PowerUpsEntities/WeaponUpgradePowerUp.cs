using Features.Tanks;

namespace Features.PowerUps.PowerUpsEntities
{
    public class WeaponUpgradePowerUp : PowerUpBase
    {
        public override bool Apply(Tank target)
        {
            if (target == null)
            {
                return false;
            }
            return target.TryUpgradeWeapon();
        }
    }
}