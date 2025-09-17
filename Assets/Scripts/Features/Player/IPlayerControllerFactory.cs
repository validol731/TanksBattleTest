using Features.Tanks;

namespace Features.Player
{
    public interface IPlayerControllerFactory
    {
        IPlayerController Create(Tank tank);
    }
}