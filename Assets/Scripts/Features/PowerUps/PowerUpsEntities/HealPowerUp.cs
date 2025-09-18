using Features.Tanks;
using UnityEngine;

namespace Features.PowerUps.PowerUpsEntities
{
    public class HealPowerUp : PowerUpBase
    {
        [SerializeField] private int healAmount = 1;
        public override bool CanConsume(Tank target)
        {
            if (target == null)
            {
                return false;
            }
            return target.State.Hp.Value < target.MaxHp;
        }

        public override void Apply(Tank target)
        {
            int current = target.State.Hp.Value;
            int max = target.MaxHp;
            int newValue = current + healAmount;
            if (newValue > max)
            {
                newValue = max;
            }

            target.State.Hp.Value = newValue;
        }
    }
}