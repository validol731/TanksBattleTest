using Features.Tanks;

namespace Features.Player
{
    public class PlayerControllerFactory : IPlayerControllerFactory
    {
        private readonly ITankInputSource _inputSource;

        public PlayerControllerFactory(ITankInputSource inputSource)
        {
            _inputSource = inputSource;
        }

        public IPlayerController Create(Tank tank)
        {
            PlayerController controller = new PlayerController(_inputSource);
            controller.Bind(tank);
            return controller;
        }
    }
}