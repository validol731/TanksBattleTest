using Features.Score;
using TMPro;
using UniRx;
using UnityEngine;
using VContainer;

namespace UI
{
    public class BestScoreLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text bestTextTMP;
        private IScoreService _score;
        private CompositeDisposable _cd = new();

        public void Initialize(IScoreService score)
        {
            _score = score;
            _cd.Clear();
            if (_score == null)
            {
                return;
            }

            _score.Best.Subscribe(UpdateBest).AddTo(_cd);
            UpdateBest(_score.Best.Value);
        }

        public void ChangeState(bool state)
        {
            bestTextTMP.gameObject.SetActive(state);
        }
        private void OnDisable()
        {
            _cd.Dispose();
        }

        private void UpdateBest(int value)
        {
            bestTextTMP.text = "BEST SCORE:\n" + value;
        }
    }
}