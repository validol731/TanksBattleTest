using UniRx;

namespace Features.Score
{
    public interface IScoreService
    {
        IReadOnlyReactiveProperty<int> Current { get; }
        IReadOnlyReactiveProperty<int> Best { get; }

        void Add(int amount, string reason = null);
        void ResetRun();
        void LoadBest();
        void SaveBest();
    }
}