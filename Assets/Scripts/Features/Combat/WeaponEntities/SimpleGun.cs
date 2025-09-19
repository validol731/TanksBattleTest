using Cysharp.Threading.Tasks;
using Features.Combat.Config;
using UnityEngine;
using UnityEngine.Pool;

namespace Features.Combat.WeaponEntities
{
    public class SimpleGun : IWeapon
    {
        private WeaponConfig.WeaponLevel _config;
        private ObjectPool<Bullet> _pool;
        private float _cooldown;

        public void Setup(WeaponConfig.WeaponLevel config, bool isEnemy)
        {
            _config = config;
            var prefab = config.playerBulletPrefab;
            if (isEnemy)
            {
                prefab = config.enemyBulletPrefab;
            }
            _pool = new ObjectPool<Bullet>(
                () => Object.Instantiate(prefab).GetComponent<Bullet>(),
                b => b.gameObject.SetActive(true),
                b => b.gameObject.SetActive(false),
                defaultCapacity: 16);
        }

        public void Tick(float dt)
        {
            _cooldown = Mathf.Max(0, _cooldown - dt);
        }

        public void TryFire(Vector2 pos, float headingRad)
        {
            if (_cooldown > 0f)
            {
                return;
            }
            _cooldown = _config.cooldown;

            int projectileCount = Mathf.Max(1, _config.projectilesPerShot);
            float totalSpreadDegrees = _config.spreadDeg;

            for (int i = 0; i < projectileCount; i++)
            {
                float spreadOffsetDegrees = 0f;

                if (projectileCount != 1)
                {
                    float t = i / (float)(projectileCount - 1);
                    float min = -totalSpreadDegrees * 0.5f;
                    float max =  totalSpreadDegrees * 0.5f;
                    spreadOffsetDegrees = Mathf.Lerp(min, max, t);
                }

                float shotRadians = headingRad + spreadOffsetDegrees * Mathf.Deg2Rad;
                var bullet = _pool.Get();
                bullet.Launch(pos, shotRadians, _config.bulletSpeed, _config.damage, () => ReleaseBullet(bullet));
                ReleaseBulletDelay(bullet).Forget();
            }
        }

        private async UniTask ReleaseBulletDelay(Bullet bullet)
        {
            await UniTask.Delay((int)(_config.bulletLife * 1000));
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