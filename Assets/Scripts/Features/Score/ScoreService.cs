using UniRx;
using UnityEngine;

namespace Features.Score
{
    public class ScoreService : IScoreService
    {
        private const string BestKey = "BestScore";

        private readonly ReactiveProperty<int> _current = new(0);
        private readonly ReactiveProperty<int> _best = new(0);

        public IReadOnlyReactiveProperty<int> Current => _current;
        public IReadOnlyReactiveProperty<int> Best => _best;

        public ScoreService()
        {
            LoadBest();
        }

        public void Add(int amount, string reason = null)
        {
            if (amount <= 0)
            {
                return;
            }

            _current.Value += amount;

            if (_current.Value > _best.Value)
            {
                _best.Value = _current.Value;
                SaveBest();
            }
        }

        public void ResetRun()
        {
            _current.Value = 0;
        }

        public void LoadBest()
        {
            int saved = PlayerPrefs.GetInt(BestKey, 0);
            _best.Value = saved;
        }

        public void SaveBest()
        {
            PlayerPrefs.SetInt(BestKey, _best.Value);
            PlayerPrefs.Save();
        }
    }
}