using UnityEngine;
using Cysharp.Threading.Tasks;
using Configs;
using Features.Tanks;

namespace Features.Spawning
{
    public class RespawnManager
    {
        private readonly BattlefieldConfig _battlefieldConfig;

        public RespawnManager(BattlefieldConfig battlefieldConfig)
        {
            _battlefieldConfig = battlefieldConfig;
        }

        public Vector2 RandomBorderPoint()
        {
            Vector2 min = _battlefieldConfig.MapMin;
            Vector2 max = _battlefieldConfig.MapMax;
            float inset = 0.64f;

            int edgeIndex = Random.Range(0, 4);
            Vector2 point = Vector2.zero;

            if (edgeIndex == 0) // left
            {
                point = new Vector2(min.x + inset, Random.Range(min.y + inset, max.y - inset));
            }
            else if (edgeIndex == 1) // right
            {
                point = new Vector2(max.x - inset, Random.Range(min.y + inset, max.y - inset));
            }
            else if (edgeIndex == 2) // bottom
            {
                point = new Vector2(Random.Range(min.x + inset, max.x - inset), min.y + inset);
            }
            else // top
            {
                point = new Vector2(Random.Range(min.x + inset, max.x - inset), max.y - inset);
            }

            return point;
        }

        public Vector2 CornerByIndex(int index)
        {
            Vector2 min = _battlefieldConfig.MapMin;
            Vector2 max = _battlefieldConfig.MapMax;
            float inset = 0.64f;

            if (index == 0)
            {
                return new Vector2(min.x + inset, min.y + inset);
            }
            else if (index == 1)
            {
                return new Vector2(max.x - inset, min.y + inset);
            }
            else if (index == 2)
            {
                return new Vector2(min.x + inset, max.y - inset);
            }
            else
            {
                return new Vector2(max.x - inset, max.y - inset);
            }
        }


        public async UniTaskVoid RespawnAfterDelay(Tank tank, Vector2 position)
        {
            float delaySeconds = _battlefieldConfig.respawnDelay;

            tank.gameObject.SetActive(false);
            await UniTask.Delay((int)(delaySeconds * 1000f));

            tank.transform.position = position;
            tank.ResetForRespawn();
        }
    }
}
