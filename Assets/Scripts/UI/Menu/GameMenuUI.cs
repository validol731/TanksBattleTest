using Features.Score;
using UI.Score;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Menu
{
    public class GameMenuUI : MenuView, IScorePopupService
    {
        protected override GameMenuType Type => GameMenuType.Game;

        [SerializeField] private Button exitButton;
        [SerializeField] private BestScoreLabel bestScoreLabel;
        [SerializeField] private ScoreLabel scoreLabel;
        
        [Header("Score Popup")]
        [SerializeField] private ScorePopupUI popupUiPrefab;
        [SerializeField] private RectTransform popupWorldRoot; 

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

        public void Show(int amount, Vector3 worldPos, Color color)
        {
            if (popupUiPrefab == null || popupWorldRoot == null)
            {
                Debug.LogWarning("[GameMenuUI] popupPrefabUI/popupWorldRoot not set");
                return;
            }
            
            var inst = Instantiate(popupUiPrefab, popupWorldRoot);
            Vector2 screen = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(popupWorldRoot, screen, Camera.main, out var local);
            inst.Play(amount, color,local);
        }
    }
}