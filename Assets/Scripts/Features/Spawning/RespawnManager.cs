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
            Vector2 min = -(_battlefieldConfig.fieldSize * 0.5f);
            Vector2 max = _battlefieldConfig.fieldSize * 0.5f;

            int edgeIndex = Random.Range(0, 4);
            Vector2 point = Vector2.zero;

            if (edgeIndex == 0)
            {
                point = new Vector2(min.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 1)
            {
                point = new Vector2(max.x, Random.Range(min.y, max.y));
            }
            else if (edgeIndex == 2)
            {
                point = new Vector2(Random.Range(min.x, max.x), min.y);
            }
            else
            {
                point = new Vector2(Random.Range(min.x, max.x), max.y);
            }

            return point;
        }

        public Vector2 CornerByIndex(int index)
        {
            Vector2 min = -(_battlefieldConfig.fieldSize * 0.5f);
            Vector2 max = _battlefieldConfig.fieldSize * 0.5f;

            if (index == 0)
            {
                return new Vector2(min.x, min.y);
            }
            else if (index == 1)
            {
                return new Vector2(max.x, min.y);
            }
            else if (index == 2)
            {
                return new Vector2(min.x, max.y);
            }
            else
            {
                return new Vector2(max.x, max.y);
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
