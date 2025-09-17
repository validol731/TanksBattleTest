using System;
using UniRx;
using UnityEngine;
using Features.Tanks;

namespace Features.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class PowerUpBase : MonoBehaviour, IPowerUpEffect
    {
        [Header("Pickup")]
        [SerializeField] private bool playerOnly = true;

        [Header("VFX (optional)")]
        [SerializeField] private GameObject pickupVfxPrefab;
        [SerializeField] private float vfxLifetime = 1.0f;

        private readonly Subject<PowerUpBase> _collected = new();
        public IObservable<PowerUpBase> Collected => _collected;

        protected virtual void Reset()
        {
            Collider2D c = GetComponent<Collider2D>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            Tank tank = other.GetComponentInParent<Tank>();
            if (tank == null)
            {
                return;
            }

            if (playerOnly)
            {
                if (tank.gameObject.CompareTag("Player") == false)
                {
                    return;
                }
            }

            bool applied = Apply(tank);
            if (applied)
            {
                PlayPickupVfx();
                _collected.OnNext(this);
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
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

        public abstract bool Apply(Tank target);
    }
}
