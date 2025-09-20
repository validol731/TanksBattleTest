using System;
using Core.Audio;
using Features.GameSave;
using Features.PowerUps;
using Features.Score;
using Features.Spawning;
using UniRx;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public abstract class MenuView : MonoBehaviour
    {
        [Header("Visual Root")]
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private AudioClip audioClick;
        [SerializeField] private AudioClip[] audioMenuMusic;

        protected abstract GameMenuType Type { get; }

        protected GameMenusController Controller;
        protected IGameSaveService Save;
        protected IScoreService Score;
        protected BattlefieldSpawner Spawner;
        protected PowerUpSpawner PowerUps;

        private CompositeDisposable _cd = new();

        private bool _isVisible;

        [Inject]
        public void Construct(GameMenusController controller, IGameSaveService save, IScoreService score, BattlefieldSpawner spawner, PowerUpSpawner powerUps)
        {
            Save = save;
            Spawner = spawner;
            PowerUps = powerUps;
            Score = score;
            Controller = controller;
        }

        private void Awake()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            _cd.Dispose();
        }

        public void PlayButtonClick()
        {
            AudioManager.Instance.PlayUI(audioClick);
        }


        private void Subscribe()
        {
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

        protected virtual void OnShown()
        {
            AudioManager.Instance.PlayMusicList(audioMenuMusic);
        }
        protected virtual void OnHidden() { }
    }
}
