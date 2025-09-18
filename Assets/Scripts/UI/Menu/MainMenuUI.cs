using Cysharp.Threading.Tasks;
using Features.PowerUps;
using Features.Score;
using Features.Spawning;
using TMPro;
using UI.Score;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VContainer;

namespace UI.Menu
{
    public class MainMenuUI : MenuView
    {
        protected override GameMenuType Type => GameMenuType.MainMenu;

        [Header("UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private BestScoreLabel bestScoreLabel;

        [Header("Countdown")]
        [SerializeField] private int countdownSeconds = 3;

        [Header("PostProcessing (optional)")]
        [SerializeField] private Volume blurVolume;

        private BattlefieldSpawner _spawner;
        private PowerUpSpawner _powerUps;

        private bool _starting;

        [Inject]
        public void Construct(GameMenusController controller, IScoreService score, BattlefieldSpawner spawner, PowerUpSpawner powerUps)
        {
            _spawner = spawner;
            _powerUps = powerUps;
            Score = score;
            Controller = controller;
        }

        private void Awake()
        {
            countdownText.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            bestScoreLabel.gameObject.SetActive(true);
            startButton.onClick.AddListener(OnStartClicked);
            bestScoreLabel.Initialize(Score);
        }

        private void OnDestroy()
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }

        protected override void OnShown()
        {
            if (blurVolume != null)
            {
                blurVolume.enabled = true;
            }
            Time.timeScale = 0f;
            startButton.gameObject.SetActive(true);
            bestScoreLabel.ChangeState(true);

            _powerUps?.StopSpawning();
            _powerUps?.DespawnAllPowerUps();
            _spawner?.DespawnAll();
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
            StartFlow().Forget();
        }

        private async UniTaskVoid StartFlow()
        {
            _spawner?.SpawnAll();
            _powerUps?.StartSpawning();
            startButton.gameObject.SetActive(false);
            bestScoreLabel.ChangeState(false);

            countdownText.gameObject.SetActive(true);
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
