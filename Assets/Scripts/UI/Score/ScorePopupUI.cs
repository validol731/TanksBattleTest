using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI.Score
{
    public class ScorePopupUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Anim")]
        [SerializeField] private float riseDistance = 60f;
        [SerializeField] private float duration = 0.9f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private Ease fadeEase = Ease.Linear;
        [SerializeField] private float startScale = 0.9f;
        [SerializeField] private float endScale = 1.1f;

        private Tween _tween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Play(int amount, Color color, Vector2 localPosition)
        {
            rectTransform.localPosition = localPosition;
            transform.localScale = Vector3.one * startScale;

            if (label != null)
            {
                if (amount >= 0)
                {
                    label.text = "+" + amount;
                }
                else
                {
                    label.text = amount.ToString();
                }
                label.color = color;
            }

            canvasGroup.alpha = 1f;

            Vector2 target = localPosition + Vector2.up * riseDistance;

            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            Sequence s = DOTween.Sequence();
            s.Join(rectTransform.DOLocalMove(target, duration).SetEase(moveEase));
            s.Join(canvasGroup.DOFade(0f, duration).SetEase(fadeEase));
            s.Join(transform.DOScale(endScale, duration * 0.6f).SetEase(Ease.OutBack));
            s.OnComplete(() => Destroy(gameObject));

            _tween = s;
        }

        private void OnDisable()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }
        }
    }
}
