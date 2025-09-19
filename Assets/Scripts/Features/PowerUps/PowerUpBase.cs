using System;
using Features.PowerUps.Config;
using Features.Score;
using UniRx;
using UnityEngine;
using Features.Tanks;
using UI.Score;
using VContainer;

namespace Features.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class PowerUpBase : MonoBehaviour, IPowerUpEffect
    {
        [SerializeField] private int aiPriority = 0;
        [SerializeField] private int scoreOnPickup = 50;

        [Header("VFX (optional)")]
        [SerializeField] private GameObject pickupVfxPrefab;
        [SerializeField] private float vfxLifetime = 1.0f;

        private readonly Subject<PowerUpBase> _collected = new Subject<PowerUpBase>();
        public IObservable<PowerUpBase> Collected => _collected;

        public PowerUpEntry currentConfig;
        private bool AllowPlayerPickup => currentConfig.allowPlayerPickup;
        private bool AllowEnemyPickup  => currentConfig.allowEnemyPickup;

        private IScoreService _score;
        private IScorePopupService _scorePopupService;

        public void Initialize(PowerUpEntry config)
        {
            currentConfig = config;
        }
        [Inject]
        public void Construct(IScoreService score, IScorePopupService scorePopupService)
        {
            _score = score;
            _scorePopupService = scorePopupService;
        }
        protected virtual void Reset()
        {
            Collider2D c = GetComponent<Collider2D>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }


        public int GetAiPriority(Tank tank)
        {
            return aiPriority;
        }

        public virtual float GetPickupDesire(Tank tank)
        {
            return 1f;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            Tank tank = other.GetComponentInParent<Tank>();
            if (tank == null)
            {
                return;
            }

            bool isPlayer = tank.gameObject.CompareTag("Player");
            if (isPlayer && !AllowPlayerPickup)
            {
                return;
            }
            if (!isPlayer && !AllowEnemyPickup)
            {
                return;
            }

            if (CanConsume(tank))
            {
                Apply(tank);
                
                if (_score != null && scoreOnPickup > 0)
                {
                    _score.Add(scoreOnPickup, "PowerUp:" + GetType().Name);
                    if (isPlayer)
                    {
                        _scorePopupService.Show(scoreOnPickup, transform.position, new Color(0.3f, 1f, 0.4f));
                    }
                }
                
                DestroyPowerUp();
            }
        }

        public bool CanBePickedBy(Tank tank)
        {
            if (tank == null)
            {
                return false;
            }

            bool isPlayer = tank.gameObject.CompareTag("Player");
            if (isPlayer)
            {
                if (AllowPlayerPickup)
                {
                    return true;
                }
                return false;
            }

            if (AllowEnemyPickup)
            {
                return true;
            }
            return false;
        }

        private void DestroyPowerUp()
        {
            PlayPickupVfx();
            _collected.OnNext(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void PlayPickupVfx()
        {
            if (pickupVfxPrefab == null)
            {
                return;
            }
            GameObject go = Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);
            if (vfxLifetime > 0f)
            {
                Destroy(go, vfxLifetime);
            }
        }

        public abstract bool CanConsume(Tank target);
        public abstract void Apply(Tank target);
    }
}
