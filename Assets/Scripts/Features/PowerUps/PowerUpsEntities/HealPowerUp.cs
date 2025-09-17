using Features.Tanks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Features.PowerUps.PowerUpsEntities
{
    public class HealPowerUp : PowerUpBase
    {
        [SerializeField] private int healAmount = 1;
        public override bool Apply(Tank target)
        {
            if (target == null)
            {
                return false;
            }

            int current = target.State.Hp.Value;
            int max = target.MaxHp;

            if (current >= max)
            {
                return false;
            }

            int newValue = current + healAmount;
            if (newValue > max)
            {
                newValue = max;
            }

            target.State.Hp.Value = newValue;
            return true;
        }
    }
}