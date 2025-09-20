using System.Collections.Generic;
using Core.GameSave;
using Features.AI;
using Features.PowerUps;

namespace Features.GameSave
{
    public class GameSaveData
    {
        public PlayerSave Player;
        public List<EnemySave> Enemies = new List<EnemySave>();
        public List<PowerUpSave> PowerUps = new List<PowerUpSave>();
    }
}