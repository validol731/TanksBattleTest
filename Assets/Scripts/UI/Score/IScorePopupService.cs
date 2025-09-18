using UnityEngine;

namespace UI.Score
{
    public interface IScorePopupService
    {
        void Show(int amount, Vector3 worldPos, Color color);
    }
}