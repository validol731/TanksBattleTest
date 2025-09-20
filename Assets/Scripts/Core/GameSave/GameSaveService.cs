using System.IO;
using Configs;
using Cysharp.Threading.Tasks;
using Features.AI;
using Features.PowerUps;
using Features.Spawning;
using UnityEngine;
using VContainer;

namespace Features.GameSave
{
    public class GameSaveService : IGameSaveService
    {
        private readonly BattlefieldConfig _battlefieldConfig;
        private readonly BattlefieldSpawner _battlefieldSpawner;
        private readonly PowerUpSpawner _powerUpSpawner;

        public GameSaveService(BattlefieldConfig battlefieldConfig, BattlefieldSpawner battlefieldSpawner, PowerUpSpawner powerUpSpawner)
        {
            _battlefieldConfig = battlefieldConfig;
            _battlefieldSpawner = battlefieldSpawner;
            _powerUpSpawner = powerUpSpawner;
        }

        public string GetSavePath(string slot)
        {
            string fileName = string.IsNullOrEmpty(slot) ? "save.json" : slot;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public GameSaveData TryGetSaveData(string slot = "save1.json")
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = "";
            try
            {
                json = File.ReadAllText(path);
            }
            catch (System.Exception ex)
            {
                return null;
            }
            
            try
            {
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public async UniTask SaveAsync(string slot = "save1.json")
        {
            GameSaveData data = CollectState();

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);

            try
            {
                File.WriteAllText(path, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[GameSaveService] Save failed: " + ex.Message);
            }

            await UniTask.Yield();
        }

        public async UniTask<bool> LoadAsync(GameSaveData data)
        {

            ApplyState(data);
            await UniTask.Yield();
            return true;
        }

        private GameSaveData CollectState()
        {
            GameSaveData save = new GameSaveData();
            PlayerSave.SaveData(save, _battlefieldSpawner);
            PowerUpSave.SaveData(save, _powerUpSpawner);
            EnemySave.SaveData(save, _battlefieldSpawner);

            return save;
        }

        private void ApplyState(GameSaveData data)
        {
            

            PlayerSave.ApplySave(_battlefieldSpawner, _battlefieldConfig, data);
            EnemySave.ApplySave(_battlefieldSpawner, _battlefieldConfig, data);
            PowerUpSave.ApplySave(_powerUpSpawner, _battlefieldConfig, data);
        }
    }
}