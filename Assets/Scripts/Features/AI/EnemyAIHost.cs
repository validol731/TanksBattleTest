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
        private readonly CompositeDisposable _disposables = new();

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
        
        private void OnDrawGizmosSelected()
        {
            if (_aiController == null)
            {
                return;
            }

            IAIDebugData debug = _aiController as IAIDebugData;
            if (debug == null)
            {
                return;
            }

            Vector2 from = transform.position;
            Vector2 to = debug.SeekTarget;

            if (debug.InEngage)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }
            Gizmos.DrawLine(new Vector3(from.x, from.y, 0f), new Vector3(to.x, to.y, 0f));

            Gizmos.color = new Color(1f, 0.6f, 0f, 1f); 
            Gizmos.DrawWireSphere(new Vector3(to.x, to.y, 0f), 0.3f);

            float heading = debug.TargetHeadingRadians;
            Vector2 dir = new Vector2(Mathf.Cos(heading), Mathf.Sin(heading));
            float arrowLen = 1.6f;

            Gizmos.color = Color.cyan;
            Vector3 tip = new Vector3(from.x + dir.x * arrowLen, from.y + dir.y * arrowLen, 0f);
            Gizmos.DrawLine(new Vector3(from.x, from.y, 0f), tip);

            Vector2 left = Quaternion.Euler(0f, 0f, 25f) * (-dir);
            Vector2 right = Quaternion.Euler(0f, 0f, -25f) * (-dir);
            float headSize = 0.35f;
            Gizmos.DrawLine(tip, tip + new Vector3(left.x, left.y, 0f) * headSize);
            Gizmos.DrawLine(tip, tip + new Vector3(right.x, right.y, 0f) * headSize);
        }
    }
}