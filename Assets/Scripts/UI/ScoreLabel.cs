using Features.Score;
using TMPro;
using UniRx;
using UnityEngine;
using VContainer;

namespace UI
{
    public class ScoreLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreTextTMP;
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

            _score.Current.Subscribe(UpdateScore).AddTo(_cd);
            UpdateScore(_score.Current.Value);
        }
        public void ChangeState(bool state)
        {
            scoreTextTMP.gameObject.SetActive(state);
        }
        private void OnDisable()
        {
            _cd.Dispose();
        }

        private void UpdateScore(int value)
        {
            scoreTextTMP.text = "SCORE:\n" + value;
        }
    }
}