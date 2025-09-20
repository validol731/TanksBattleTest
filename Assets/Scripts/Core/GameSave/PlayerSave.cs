using System;
using Configs;
using Features.AI;
using Features.Spawning;
using UnityEngine;

namespace Features.GameSave
{
    [Serializable]
    public class PlayerSave
    {
        public Vector2 position;
        public float rotationZ;
        public int hp;
        public int weaponLevel;

        public static void SaveData(GameSaveData save, BattlefieldSpawner battlefieldSpawner)
        {
            var player = battlefieldSpawner.GetPlayer();
            PlayerSave playerSave = new PlayerSave();
            playerSave.position = player.transform.position;
            playerSave.rotationZ = player.transform.eulerAngles.z;
            playerSave.hp = player.CurrentHp;
            playerSave.weaponLevel = player.WeaponLevelIndex;
            save.Player = playerSave;
        }

        public static void ApplySave(BattlefieldSpawner battlefieldSpawner, BattlefieldConfig battlefieldConfig, GameSaveData data)
        {
            var player = data.Player;
            if (player.hp == 0)
            {
                battlefieldSpawner.SpawnPlayerNewGame();
            }
            else
            {
                Quaternion rot = Quaternion.Euler(0f, 0f, player.rotationZ);
                battlefieldSpawner.SpawnPlayer(battlefieldConfig.playerConfig, player.position, rot, player.hp, player.weaponLevel);   
            }
        }
    }
}