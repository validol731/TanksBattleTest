using Features.Tanks;

namespace Features.PowerUps
{
    public interface IPowerUpEffect
    {
        bool Apply(Tank target);
    }
}