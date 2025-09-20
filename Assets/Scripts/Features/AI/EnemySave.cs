using System;
using System.Linq;
using Configs;
using Features.AI.Config;
using Features.GameSave;
using Features.Spawning;
using UnityEngine;

namespace Features.AI
{
    [Serializable]
    public class EnemySave
    {
        public string id;
        public Vector2 position;
        public float rotationZ;
        public int hp;
        public int weaponLevel;

        public static void SaveData(GameSaveData save, BattlefieldSpawner battlefieldSpawner)
        {
            foreach (var enemy in battlefieldSpawner.GetEnemies())
            {
                EnemySave es = new EnemySave();
                es.position = enemy.transform.position;
                es.rotationZ = enemy.transform.eulerAngles.z;
                es.hp = enemy.CurrentHp;
                es.weaponLevel = enemy.WeaponLevelIndex;
                es.id = enemy.Config.id;
                save.Enemies.Add(es);
            }
        }

        public static void ApplySave(BattlefieldSpawner battlefieldSpawner, BattlefieldConfig battlefieldConfig, GameSaveData data)
        {
            for (int i = 0; i < data.Enemies.Count; i++)
            {
                EnemySave es = data.Enemies[i];
                AITankConfig config = ResolveEnemyConfig(battlefieldConfig, es.id);
                Quaternion rot = Quaternion.Euler(0f, 0f, es.rotationZ);
                battlefieldSpawner.SpawnEnemy(config, es.position, rot, es.hp, es.weaponLevel);
            }
        }
        
        private static AITankConfig ResolveEnemyConfig(BattlefieldConfig battlefieldConfig, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (battlefieldConfig.enemies != null)
            {
                for (int i = 0; i < battlefieldConfig.enemies.Count; i++)
                {
                    AITankConfig config = battlefieldConfig.enemies[i].tankConfig;
                    if (config == null)
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(config.id) && config.id == id)
                    {
                        return config;
                    }
                    if (config.name == id)
                    {
                        return config;
                    }
                }
            }
            return null;
        }
    }
}