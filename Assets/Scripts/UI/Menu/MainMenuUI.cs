using Core.Audio;
using Cysharp.Threading.Tasks;
using Features.GameSave;
using TMPro;
using UI.Score;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UI.Menu
{
    public class MainMenuUI : MenuView
    {
        protected override GameMenuType Type => GameMenuType.MainMenu;

        [Header("UI")]
        [SerializeField] private Button newStartButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private BestScoreLabel bestScoreLabel;
        [SerializeField] private AudioClip timerAudio;

        [Header("Countdown")]
        [SerializeField] private int countdownSeconds = 3;

        [Header("PostProcessing (optional)")]
        [SerializeField] private Volume blurVolume;

        private bool _starting;
        private GameSaveData _saveData;

        private void Start()
        {
            newStartButton.onClick.AddListener(OnStartClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDestroy()
        {
            newStartButton.onClick.RemoveListener(OnStartClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        protected override void OnShown()
        {
            base.OnShown();
            if (blurVolume != null)
            {
                blurVolume.enabled = true;
            }

            bestScoreLabel.Initialize(Score);
            _saveData = Save.TryGetSaveData();
            Time.timeScale = 0f;
            newStartButton.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(_saveData != null);
            bestScoreLabel.ChangeState(true);

            Score?.ResetRun();
        }

        protected override void OnHidden()
        {
            if (blurVolume != null)
            {
                blurVolume.enabled = false;
            }
            Time.timeScale = 1f;
        }

        private void OnStartClicked()
        {
            if (_starting)
            {
                return;
            }

            _starting = true;
            StartFlow(false).Forget();
        }
        private void OnContinueClicked()
        {
            if (_starting)
            {
                return;
            }

            _starting = true;
            StartFlow(true).Forget();
        }

        private async UniTaskVoid StartFlow(bool loadData)
        {
            newStartButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            bestScoreLabel.ChangeState(false);

            countdownText.gameObject.SetActive(true);
            if (loadData && _saveData != null)
            {
                Save.LoadAsync(_saveData).Forget();
            }
            else
            {
                Spawner?.SpawnNewGame();
            }
            PowerUps?.StartSpawning();
            AudioManager.Instance.PlayUI(timerAudio);
            for (int t = countdownSeconds; t >= 1; t--)
            {
                countdownText.text = t.ToString();
                await UniTask.Delay(1000, ignoreTimeScale: true);
            }
            countdownText.text = "GO!";
            await UniTask.Delay(500, ignoreTimeScale: true);
            countdownText.gameObject.SetActive(false);

            Controller?.SetMenu(GameMenuType.Game);

            _starting = false;
        }
    }
}
