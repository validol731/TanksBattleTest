using UniRx;

namespace Features.Tanks
{
    public class TankState
    {
        public readonly ReactiveProperty<int> Hp;
        public readonly ReactiveProperty<bool> IsAlive;
        public readonly ReactiveProperty<int> WeaponLevelIndex;

        public TankState(int maxHp, int weaponLevelIndex)
        {
            Hp = new ReactiveProperty<int>(maxHp);
            WeaponLevelIndex = new ReactiveProperty<int>(weaponLevelIndex);
            IsAlive = new ReactiveProperty<bool>(true);
        }
    }
}