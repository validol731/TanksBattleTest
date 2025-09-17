using Features.Tanks;
using UnityEngine;

namespace Features.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private System.Action _onDone;
        private int _damage;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>(); 
        }

        public void Launch(Vector2 pos, float headingRad, float speed, int damage, System.Action onDone)
        {
            _onDone = onDone;
            _damage = damage;
            transform.position = pos;
            transform.rotation = Quaternion.Euler(0,0, headingRad * Mathf.Rad2Deg);
            _rigidbody.velocity = new Vector2(Mathf.Cos(headingRad), Mathf.Sin(headingRad)) * speed;
        }

        private void OnCollisionEnter2D(Collision2D c)
        {
            if (c.collider.TryGetComponent<IDamageable>(out var d))
            {
                d.TakeHit(_damage);
            }
            _onDone?.Invoke();
        }
    }
}