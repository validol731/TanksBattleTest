using Features.Tanks;

namespace Features.PowerUps
{
    public interface IPowerUpEffect
    {
        bool CanConsume(Tank target);
        void Apply(Tank target);
    }
}