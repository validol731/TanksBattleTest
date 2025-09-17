using Cysharp.Threading.Tasks;
using Features.Spawning;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VContainer;

namespace MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text countdownText;

        [Header("PostProcessing (optional)")]
        [SerializeField] private Volume blurVolume;

        [Header("Countdown")]
        [SerializeField] private int countdownSeconds = 3;

        private BattlefieldSpawner _spawner;
        private bool _starting;

        [Inject]
        public void Construct(BattlefieldSpawner spawner)
        {
            _spawner = spawner;
        }

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
            if (blurVolume != null)
            {
                blurVolume.enabled = true;
            }

            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartClicked);
            }
        }

        private void OnStartClicked()
        {
            if (_starting)
            {
                return;
            }
            _starting = true;
            StartGameFlow().Forget();
        }

        private async UniTaskVoid StartGameFlow()
        {
            if (_spawner != null)
            {
                _spawner.SpawnAll();
            }

            if (blurVolume != null)
            {
                blurVolume.enabled = false;
            }

            Time.timeScale = 0f;

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                for (int t = countdownSeconds; t >= 1; t--)
                {
                    countdownText.text = t.ToString();
                    await UniTask.Delay(1000, ignoreTimeScale: true);
                }

                countdownText.text = "GO!";
                await UniTask.Delay(500, ignoreTimeScale: true);
                countdownText.gameObject.SetActive(false);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }

            Time.timeScale = 1f;
        }
    }
}
