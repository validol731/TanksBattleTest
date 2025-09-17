using Features.Tanks;
using UnityEngine;
using VContainer;

namespace Features.Player
{
    [RequireComponent(typeof(Tank))]
    public class PlayerControllerHost : MonoBehaviour
    {
        private IPlayerControllerFactory _factory;
        private IPlayerController _controller;
        private Tank _tank;

        [Inject]
        public void Construct(IPlayerControllerFactory factory)
        {
            _factory = factory;
        }

        private void Awake()
        {
            _tank = GetComponent<Tank>();
        }

        private void Start()
        {
            _controller = _factory.Create(_tank);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.Dispose();
            }
        }
    }
}