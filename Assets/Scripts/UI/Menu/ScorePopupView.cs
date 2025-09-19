using UI.Score;
using UnityEngine;

namespace UI.Menu
{
    public class ScorePopupView: MonoBehaviour, IScorePopupService
    {
        [SerializeField] private ScorePopupUI popupUiPrefab;
        [SerializeField] private RectTransform popupWorldRoot;
        public void Show(int amount, Vector3 worldPos, Color color)
        {
            if (popupUiPrefab == null || popupWorldRoot == null)
            {
                Debug.LogWarning("[ScorePopupView] popupPrefabUI/popupWorldRoot not set");
                return;
            }
            var inst = Instantiate(popupUiPrefab, popupWorldRoot);
            Vector2 screen = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(popupWorldRoot, screen, Camera.main, out var local);
            inst.Play(amount, color,local);
        }
    }
}