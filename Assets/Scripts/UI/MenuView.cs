using Features.Score;
using UniRx;
using UnityEngine;
using VContainer;

namespace UI
{
    public abstract class MenuView : MonoBehaviour
    {
        [Header("Visual Root")]
        [SerializeField] private CanvasGroup canvasGroup;

        protected abstract GameMenuType Type { get; }

        protected GameMenusController Controller;
        protected IScoreService Score;
        private CompositeDisposable _cd = new();

        private bool _isVisible;
      

        private void OnEnable()
        {
            if (Controller == null)
            {
                Controller = FindObjectOfType<GameMenusController>();
            }
            Subscribe();
        }

        private void OnDisable()
        {
            _cd.Dispose();
        }

        protected void Subscribe()
        {
            if (Controller == null)
            {
                return;
            }

            _cd.Dispose();
            _cd = new CompositeDisposable();

            Controller.Current
                .Subscribe(OnMenuChangedInternal)
                .AddTo(_cd);

            OnMenuChangedInternal(Controller.Current.Value);
        }

        private void OnMenuChangedInternal(GameMenuType newType)
        {
            bool shouldBeVisible = newType == Type;

            if (shouldBeVisible && !_isVisible)
            {
                ShowMenu();
                OnShown();
                _isVisible = true;
            }
            else if (!shouldBeVisible && _isVisible)
            {
                OnHidden();
                HideMenu();
                _isVisible = false;
            }
        }

        protected virtual void ShowMenu()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        protected virtual void HideMenu()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
