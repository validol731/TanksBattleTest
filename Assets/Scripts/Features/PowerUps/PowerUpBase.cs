using System;
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

        private bool _allowPlayerPickup = true;
        private bool _allowEnemyPickup  = false;

        private IScoreService _score;
        private IScorePopupService _scorePopupService;

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

        public void ConfigurePickupPermissions(bool allowPlayer, bool allowEnemy)
        {
            _allowPlayerPickup = allowPlayer;
            _allowEnemyPickup = allowEnemy;
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
            if (isPlayer && !_allowPlayerPickup)
            {
                return;
            }
            if (!isPlayer && !_allowEnemyPickup)
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
                if (_allowPlayerPickup)
                {
                    return true;
                }
                return false;
            }

            if (_allowEnemyPickup)
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
