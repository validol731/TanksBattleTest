using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Features.Combat.WeaponEntities
{
    public class SimpleGun : IWeapon
    {
        private WeaponRuntimeConfig _config;
        private ObjectPool<Bullet> _pool;
        private float _cooldown;

        public void Setup(WeaponRuntimeConfig config)
        {
            _config = config;
            _pool = new ObjectPool<Bullet>(
                () => GameObject.Instantiate(_config.BulletPrefab).GetComponent<Bullet>(),
                b => b.gameObject.SetActive(true),
                b => b.gameObject.SetActive(false),
                defaultCapacity: 16);
        }

        public void Tick(float dt) { _cooldown = Mathf.Max(0, _cooldown - dt); }

        public void TryFire(Vector2 pos, float headingRad)
        {
            if (_cooldown > 0f)
            {
                return;
            }
            _cooldown = _config.Cooldown;

            int n = Mathf.Max(1, _config.ProjectilesPerShot);
            float spread = _config.SpreadDeg;
            for (int i = 0; i < n; i++)
            {
                float t = 0f;
                if (n != 1)
                {
                    t = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (float)(n - 1));
                }
                float rad = headingRad + t * Mathf.Deg2Rad;

                var bullet = _pool.Get();
                bullet.Launch(pos, rad, _config.BulletSpeed, _config.Damage, () => ReleaseBullet(bullet));
                ReleaseBulletDelay(bullet).Forget();
            }
        }

        private async UniTask ReleaseBulletDelay(Bullet bullet)
        {
            await UniTask.Delay((int)(_config.BulletLife * 1000));
            ReleaseBullet(bullet);
        }
        private void ReleaseBullet(Bullet bullet)
        {
            if (bullet.gameObject.activeSelf)
            {
                _pool.Release(bullet);
            }
        }
    }
}