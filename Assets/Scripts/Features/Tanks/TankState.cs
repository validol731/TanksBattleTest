using UniRx;

namespace Features.Tanks
{
    public class TankState
    {
        public readonly ReactiveProperty<int> Hp;
        public readonly ReactiveProperty<bool> IsAlive;

        public TankState(int maxHp)
        {
            Hp = new ReactiveProperty<int>(maxHp);
            IsAlive = new ReactiveProperty<bool>(true);
        }
    }
}