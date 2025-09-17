// Features/UI/HeartsView2D.cs
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Features.Tanks;

namespace Features.UI
{
    public class HeartsView2D : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;

        [Header("Layout")]
        [SerializeField] private float spacing = 0.32f;
        [SerializeField] private Vector2 localOffset = new(0f, 1.0f);
        [SerializeField] private bool centerAlign = true;
        [SerializeField] private float heartScale = 0.5f;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "UIWorld";
        [SerializeField] private int sortingOrder = 100;
        [SerializeField] private bool freezeWorldRotation = true;

        private readonly List<SpriteRenderer> _icons = new();
        private CompositeDisposable _disposables = new();

        private Tank _targetTank;

        public void Initialize(Tank tank)
        {
            _targetTank = tank;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            _disposables.Dispose();
        }

        private void LateUpdate()
        {
            if (_targetTank == null)
            {
                return;
            }

            transform.position = (Vector2)_targetTank.transform.position + localOffset;

            if (freezeWorldRotation)
            {
                transform.rotation = Quaternion.identity;
            }
        }

        private void Subscribe()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            if (_targetTank == null)
            {
                return;
            }

            RebuildIcons(_targetTank.MaxHp);
            RefreshIcons(_targetTank.State.Hp.Value, _targetTank.MaxHp);

            _targetTank.State.Hp
                .Subscribe(hp => RefreshIcons(hp, _targetTank.MaxHp))
                .AddTo(_disposables);
        }

        private void RebuildIcons(int max)
        {
            for (int i = _icons.Count - 1; i >= max; i--)
            {
                if (_icons[i] != null)
                {
                    Destroy(_icons[i].gameObject);
                }
                _icons.RemoveAt(i);
            }

            for (int i = _icons.Count; i < max; i++)
            {
                var go = new GameObject("Heart_" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
                _icons.Add(sr);
            }

            float startX = 0f;
            if (centerAlign)
            {
                startX = -0.5f * (max - 1) * spacing;
            }

            for (int i = 0; i < _icons.Count; i++)
            {
                var t = _icons[i].transform;
                t.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
                t.localScale = Vector3.one * Mathf.Max(0.01f, heartScale);
            }
        }

        private void RefreshIcons(int hp, int max)
        {
            if (_icons.Count != max)
            {
                RebuildIcons(max);
            }

            for (int i = 0; i < _icons.Count; i++)
            {
                var sr = _icons[i];
                if (sr == null)
                {
                    continue;
                }

                if (i < hp)
                {
                    sr.sprite = fullHeart;
                    sr.enabled = true;
                }
                else
                {
                    sr.sprite = emptyHeart != null ? emptyHeart : fullHeart;
                    sr.enabled = true;
                }
            }
        }
    }
}
