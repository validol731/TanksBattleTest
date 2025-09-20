using Cysharp.Threading.Tasks;

namespace Features.GameSave
{
    public interface IGameSaveService
    {
        UniTask SaveAsync(string slot = "save1.json");
        UniTask<bool> LoadAsync(GameSaveData data);
        GameSaveData TryGetSaveData(string slot = "save1.json");
    }
}