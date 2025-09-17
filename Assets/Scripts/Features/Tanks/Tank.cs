using System;
using Features.Combat;
using Features.Movement;
using Features.Tanks.Config;
using Features.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Features.Tanks
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Tank : MonoBehaviour, IDamageable
    {
        [SerializeField] private SpriteRenderer image;
        [SerializeField] private HeartsView2D heartsView2D;
        
        private TankConfig _config;
        private Rigidbody2D _rigidbody2D;
        private TankState _state;
        private IMovementController _movementController;
        private IWeapon _weapon;
        private IWeaponFactory _weaponFactory;

        private readonly Subject<Tank> _diedSubject = new();
        public IObservable<Tank> Died => _diedSubject;
        public IMovementController Movement => _movementController;
        public TankState State => _state;
        public int MaxHp => _config.maxHp;
        public float RespawnDelay => _config.respawnDelay;
        private bool IsEnemy => _config.IsEnemy;
        private int _weaponLevelIndex = 0;

        [Inject]
        public void Construct(IMovementController movementController, IWeaponFactory weaponFactory)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _movementController = movementController;
            _weaponFactory = weaponFactory;
        }

        private void Awake()
        {
            if (_config != null)
            {
                ConfigureFromStats();
            }
        }

        public void Initialize(TankConfig stats)
        {
            _config = stats;
            image.sprite = _config.image;
            ConfigureFromStats();
            heartsView2D.Initialize(this);
        }

        private void ConfigureFromStats()
        {
            if (_config == null)
            {
                Debug.LogError("[Tank] Stats is null. Assign via inspector or call Initialize(stats).");
                return;
            }

            _state = new TankState(_config.maxHp);
            _movementController.Setup(_rigidbody2D, _config);
            BuildWeaponFromSlot();
        }

        private void BuildWeaponFromSlot()
        {
            _weapon = _weaponFactory.Build(_config.weaponConfig, _weaponLevelIndex, _config.IsEnemy);
        }

        public void ResetForRespawn()
        {
            _state.Hp.Value = _config.maxHp;
            _state.IsAlive.Value = true;
            gameObject.SetActive(true);
        }

        public void TryFire()
        {
            if (_weapon == null)
            {
                return;
            }

            _weapon.TryFire(transform.position, _movementController.CurrentHeadingRad);
        }

        public void TickWeapon(float deltaTime)
        {
            if (_weapon == null)
            {
                return;
            }

            _weapon.Tick(deltaTime);
        }

        public bool CanUpgradeWeapon()
        {
            if (_config == null)
            {
                return false;
            }
            if (_config.weaponConfig == null)
            {
                return false;
            }
            int max = _config.weaponConfig.levels.Count - 1;
            return _weaponLevelIndex < max;
        }

        public void UpgradeWeapon()
        {
            _weaponLevelIndex += 1;
            BuildWeaponFromSlot();
        }
        private void OnCollisionEnter2D(Collision2D c)
        {
            if (c.collider.TryGetComponent<IDamageable>(out var d) && d is Tank tank)
            {
                if (tank.IsEnemy && !IsEnemy)
                {
                    TakeHit(_state.Hp.Value);
                }
            }
        }


        public void TakeHit(int damage)
        {
            if (_state.IsAlive.Value == false)
            {
                return;
            }

            _state.Hp.Value -= damage;

            if (_state.Hp.Value <= 0)
            {
                _state.IsAlive.Value = false;
                _weaponLevelIndex = 0;
                _diedSubject.OnNext(this);
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            TickWeapon(Time.deltaTime);
        }
    }
}
