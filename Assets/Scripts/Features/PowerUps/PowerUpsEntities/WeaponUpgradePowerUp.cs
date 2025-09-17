using Features.Tanks;

namespace Features.PowerUps.PowerUpsEntities
{
    public class WeaponUpgradePowerUp : PowerUpBase
    {
        public override bool CanConsume(Tank target)
        {
            if (target == null)
            {
                return false;
            }
            return target.CanUpgradeWeapon();
        }
        public override void Apply(Tank target)
        {
            target.UpgradeWeapon();
        }
    }
}