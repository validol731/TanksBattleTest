using DG.Tweening;
using UnityEngine;

namespace Features.PowerUps
{
    public class PowerUpIdleFloat : MonoBehaviour
    {
        [Header("Float")] 
        [SerializeField] private float amplitude = 0.25f;
        [SerializeField] private float period = 1.6f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private bool useLocal = true;

        [Header("Desync")]
        [SerializeField, Range(0f, 1f)] private float randomPhase01 = 0.5f;

        private Tween _tween;
        private float _baseYLocal;
        private float _baseYWorld;

        private void OnEnable()
        {
            _baseYLocal = transform.localPosition.y;
            _baseYWorld = transform.position.y;

            KillTween();

            float half = Mathf.Max(0.01f, period * 0.5f);
            float fromY, toY;

            if (useLocal)
            {
                fromY = _baseYLocal - amplitude;
                toY   = _baseYLocal + amplitude;
                transform.localPosition = new Vector3(transform.localPosition.x, fromY, transform.localPosition.z);
                _tween = transform.DOLocalMoveY(toY, half).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                fromY = _baseYWorld - amplitude;
                toY   = _baseYWorld + amplitude;
                transform.position = new Vector3(transform.position.x, fromY, transform.position.z);
                _tween = transform.DOMoveY(toY, half).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
            }

            if (randomPhase01 > 0f)
            {
                float offset = Random.Range(0f, randomPhase01) * period;
                _tween.Goto(offset, true);
            }
        }

        private void OnDisable()
        {
            KillTween();
            if (useLocal)
            {
                var p = transform.localPosition;
                transform.localPosition = new Vector3(p.x, _baseYLocal, p.z);
            }
            else
            {
                var p = transform.position;
                transform.position = new Vector3(p.x, _baseYWorld, p.z);
            }
        }

        private void KillTween()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
                _tween = null;
            }
        }
    }
}
