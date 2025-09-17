using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Features.Player
{
    public class KeyboardMouseTankInput : MonoBehaviour, ITankInputSource
    {
        private readonly Subject<Vector2> _moveSubject = new();
        private readonly Subject<Unit> _fireSubject = new();

        public IObservable<Vector2> Move => _moveSubject;
        public IObservable<Unit> Fire => _fireSubject;

        private void Start()
        {
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float forward = 0f;
                    float turn = 0f;

                    if (Input.GetKey(KeyCode.W))
                    {
                        forward += 1f;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        forward -= 1f;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        turn -= 1f;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        turn += 1f;
                    }

                    _moveSubject.OnNext(new Vector2(turn, forward));
                    if (Input.GetMouseButtonDown(0))
                    {
                        _fireSubject.OnNext(Unit.Default);
                    }
                })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            _moveSubject.OnCompleted();
            _moveSubject.Dispose();
            _fireSubject.OnCompleted();
            _fireSubject.Dispose();
        }
    }
}