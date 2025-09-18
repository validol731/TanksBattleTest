using Configs;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    [ExecuteAlways]
    public class ArenaBuilder : MonoBehaviour
    {
        [SerializeField] private BattlefieldConfig battlefieldConfig;

        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;

        [Header("Tiling")]
        [SerializeField] private float tileSize = 0.64f;
        [SerializeField] private int wallThicknessInTiles = 1;
        [SerializeField] private float wallPossOffset = 0.2f;

        [Header("Layers")]
        [SerializeField] private string wallsLayerName = "Walls";

        private const string FloorName = "_Floor";
        private const string LeftWallName = "_Wall_Left";
        private const string RightWallName = "_Wall_Right";
        private const string BottomWallName = "_Wall_Bottom";
        private const string TopWallName = "_Wall_Top";

        [ContextMenu("Rebuild Arena")]
        public void RebuildArena()
        {
            if (battlefieldConfig == null)
            {
                Debug.LogError("[ArenaBuilder] BattlefieldConfig is not assigned.");
                return;
            }
            if (floorPrefab == null || wallPrefab == null)
            {
                Debug.LogError("[ArenaBuilder] Floor or Wall prefab is not assigned.");
                return;
            }

            float width = SnapToGrid(battlefieldConfig.MapWidth);
            float height = SnapToGrid(battlefieldConfig.MapHeight);
            Vector2 center = battlefieldConfig.mapCenter;

            float wallThickness = wallThicknessInTiles * tileSize;

            // Floor
            GameObject floor = FindOrCreateChild(FloorName, floorPrefab);
            SetTransform(floor.transform, center, 0f);
            SetTiledSize(floor, width, height, hasCollider: false);

            // Walls
            int wallsLayer = LayerMask.NameToLayer(wallsLayerName);
            if (wallsLayer < 0)
            {
                Debug.LogWarning($"[ArenaBuilder] Layer \"{wallsLayerName}\" not found. Using current layer.");
            }

            // Left
            Vector2 leftPos = new Vector2(battlefieldConfig.MapMin.x - wallPossOffset, center.y);
            GameObject left = FindOrCreateChild(LeftWallName, wallPrefab, wallsLayer);
            SetTransform(left.transform, leftPos, 0f);
            SetTiledSize(left, wallThickness, height + wallThickness * 2f, hasCollider: true);

            // Right
            Vector2 rightPos = new Vector2(battlefieldConfig.MapMax.x + wallPossOffset, center.y);
            GameObject right = FindOrCreateChild(RightWallName, wallPrefab, wallsLayer);
            SetTransform(right.transform, rightPos, 0f);
            SetTiledSize(right, wallThickness, height + wallThickness * 2f, hasCollider: true);

            // Bottom
            Vector2 bottomPos = new Vector2(center.x, battlefieldConfig.MapMin.y - Mathf.Abs(center.y) - wallPossOffset);
            GameObject bottom = FindOrCreateChild(BottomWallName, wallPrefab, wallsLayer);
            SetTransform(bottom.transform, bottomPos, 0f);
            SetTiledSize(bottom, width + wallThickness * 2f, wallThickness, hasCollider: true);

            // Top
            Vector2 topPos = new Vector2(center.x, battlefieldConfig.MapMax.y + center.y + wallPossOffset);
            GameObject top = FindOrCreateChild(TopWallName, wallPrefab, wallsLayer);
            SetTransform(top.transform, topPos, 0f);
            SetTiledSize(top, width + wallThickness * 2f, wallThickness, hasCollider: true);
        }

        private float SnapToGrid(float value)
        {
            if (tileSize <= 0f)
            {
                return value;
            }
            float tiles = Mathf.Round(value / tileSize);
            return tiles * tileSize;
        }

        private GameObject FindOrCreateChild(string childName, GameObject prefab, int setLayer = -1)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                GameObject go = child.gameObject;
                if (setLayer >= 0)
                {
                    go.layer = setLayer;
                }
                return go;
            }

#if UNITY_EDITOR
            GameObject instance;
            if (!Application.isPlaying)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
            }
            else
            {
                instance = Instantiate(prefab, transform);
            }
#else
        GameObject instance = Instantiate(prefab, transform);
#endif
            instance.name = childName;
            if (setLayer >= 0)
            {
                instance.layer = setLayer;
            }
            return instance;
        }

        private void SetTransform(Transform t, Vector2 position, float rotationZ)
        {
            t.position = new Vector3(position.x, position.y, 0f);
            t.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            t.localScale = Vector3.one;
        }

        private void SetTiledSize(GameObject go, float sizeX, float sizeY, bool hasCollider)
        {
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = go.AddComponent<SpriteRenderer>();
            }

            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(sizeX, sizeY);

            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            if (hasCollider)
            {
                if (box == null)
                {
                    box = go.AddComponent<BoxCollider2D>();
                }
                box.isTrigger = false;
                box.size = new Vector2(sizeX, sizeY);
                box.offset = Vector2.zero;
            }
            else
            {
                if (box != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        DestroyImmediate(box);
                    }
                    else
                    {
                        Destroy(box);
                    }
#else
                Destroy(box);
#endif
                }
            }
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            if (battlefieldConfig == null)
            {
                return;
            }
            RebuildArena();
        }
    }
}
