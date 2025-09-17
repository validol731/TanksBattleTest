using System;
using Features.Combat;
using Features.Movement;
using Features.Tanks.Config;
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
        
        private TankConfig _config;
        private Rigidbody2D _rigidbody2D;
        private TankState _state;
        private IMovementController _movementController;
        private IWeapon _weapon;
        private IWeaponFactory _weaponFactory;

        private readonly Subject<Tank> _diedSubject = new();
        public IObservable<Tank> Died => _diedSubject;
        public IMovementController Movement => _movementController;

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
            TankConfig.WeaponSlot slot = _config.weapon;
            if (slot.config != null)
            {
                _weapon = _weaponFactory.Build(slot.config, slot.levelIndex, _config.isEnemy);
            }
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

        public void UpgradeWeapon()
        {
            if (_config.weapon.config == null)
            {
                return;
            }

            int maxLevelIndex = _config.weapon.config.levels.Count - 1;
            int nextLevelIndex = _config.weapon.levelIndex + 1;
            if (nextLevelIndex > maxLevelIndex)
            {
                nextLevelIndex = maxLevelIndex;
            }

            _config.weapon.levelIndex = nextLevelIndex;
            BuildWeaponFromSlot();
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
