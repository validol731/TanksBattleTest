using Cysharp.Threading.Tasks;
using UI.Score;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class GameMenuUI : MenuView
    {
        protected override GameMenuType Type => GameMenuType.Game;

        [SerializeField] private Button exitButton;
        [SerializeField] private BestScoreLabel bestScoreLabel;
        [SerializeField] private ScoreLabel scoreLabel;
        
        private bool _isInProcess = false;
        
        
        private void Start()
        {
            exitButton.onClick.AddListener(OnExitClicked);
            bestScoreLabel.Initialize(Score);
            scoreLabel.Initialize(Score);
        }
        
        private void OnDestroy()
        {
            
            exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnExitClicked()
        {
            SaveAnExit().Forget();
        }

        private async UniTask SaveAnExit()
        {
            //todo loading overlay
            if (_isInProcess)
            {
                return;
            }
            _isInProcess = true;
            await Save.SaveAsync();
            Spawner.DespawnAll();
            PowerUps.StopSpawning();
            PowerUps.DespawnAllPowerUps();
            Controller?.SetMenu(GameMenuType.MainMenu);
            _isInProcess = false;
        }
        
        
    }
}