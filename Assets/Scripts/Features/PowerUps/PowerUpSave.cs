using System;
using System.Linq;
using Configs;
using Features.GameSave;
using Features.PowerUps.Config;
using UnityEngine;

namespace Features.PowerUps
{
    [Serializable]
    public class PowerUpSave
    {
        public string id;
        public Vector2 position;


        public static void SaveData(GameSaveData save, PowerUpSpawner powerUpSpawner)
        {
            foreach (var powerUp in powerUpSpawner.GetSpawned())
            {
                PowerUpSave s = new PowerUpSave();
                s.position = powerUp.transform.position;
                s.id = powerUp.id;
                save.PowerUps.Add(s);
            }
        }
        
        public static void ApplySave(PowerUpSpawner powerUpSpawner, BattlefieldConfig config, GameSaveData data)
        {
            foreach (var powerUpSave in data.PowerUps)
            {
                PowerUpEntry powerUpEntry = config.powerUpSpawnConfig.items.FirstOrDefault(x => x.id == powerUpSave.id);
                if (powerUpEntry != null)
                {
                    powerUpSpawner.SpawnPowerUp(powerUpEntry, powerUpSave.position);
                }
            }
        }
        
    }
}