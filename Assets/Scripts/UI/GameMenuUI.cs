using Features.Score;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class GameMenuUI : MenuView
    {
        protected override GameMenuType Type => GameMenuType.Game;

        [SerializeField] private Button exitButton;
        [SerializeField] private BestScoreLabel bestScoreLabel;
        [SerializeField] private ScoreLabel scoreLabel;

        [Inject]
        public void BaseConstruct(GameMenusController controller, IScoreService score)
        {
            Controller = controller;
            Score = score;
            Subscribe();
        }
        private void Awake()
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
            Controller?.SetMenu(GameMenuType.MainMenu);
        }
    }
}