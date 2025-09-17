using UnityEngine;
using VContainer;
using UniRx;
using UniRx.Triggers;
using Configs;
using Features.Tanks;

namespace Features.AI
{
    [RequireComponent(typeof(Tank), typeof(Rigidbody2D))]
    public class EnemyAIHost : MonoBehaviour
    {
        private IAIController _aiController;
        private BattlefieldConfig _battlefieldConfig;

        private Tank _tank;
        private Rigidbody2D _rigidbody2D;
        private CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public void Construct(IAIController aiController, BattlefieldConfig battlefieldConfig)
        {
            _aiController = aiController;
            _battlefieldConfig = battlefieldConfig;
        }

        private void Awake()
        {
            _tank = GetComponent<Tank>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _aiController.Setup(_tank.Movement, _battlefieldConfig, _rigidbody2D);

            Observable.EveryFixedUpdate()
                .Subscribe(_ => _aiController.Tick(Time.fixedDeltaTime))
                .AddTo(_disposables);

            this.OnCollisionEnter2DAsObservable()
                .Subscribe(_ => _aiController.OnCollision())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}