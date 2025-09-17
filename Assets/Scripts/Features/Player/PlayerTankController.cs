using Features.Tanks;
using UniRx;
using UnityEngine;

namespace Features.Player
{
    public class PlayerController : IPlayerController
    {
        private readonly ITankInputSource _inputSource;
        private readonly CompositeDisposable _disposables = new();

        private Tank _tank;
        private Vector2 _latestMove; // x = turn, y = forward

        public PlayerController(ITankInputSource inputSource)
        {
            _inputSource = inputSource;
        }

        public void Bind(Tank tank)
        {
            _tank = tank;
            _inputSource.Move
                .Subscribe(value => _latestMove = value)
                .AddTo(_disposables);

            Observable.EveryFixedUpdate()
                .Subscribe(_ =>
                {
                    float turn = _latestMove.x;
                    float forward = _latestMove.y;
                    _tank.Movement.Move(forward, turn, Time.fixedDeltaTime);
                })
                .AddTo(_disposables);

            _inputSource.Fire
                .Subscribe(_ => _tank.TryFire())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}