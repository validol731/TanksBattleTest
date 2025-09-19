using Features.Combat.Config;
using Features.Movement.Config;
using UnityEngine;

namespace Features.Tanks.Config
{
    [CreateAssetMenu(menuName = "Configs/Tanks/TankConfig")]
    public class TankConfig : ScriptableObject
    {
        [SerializeField] public string id;
        public virtual bool IsEnemy { get; set; } = false;
        [SerializeField] public Sprite image;
        [SerializeField] public int maxHp = 1;
        [SerializeField] public float respawnDelay = 1.0f;
        [SerializeField] public WeaponConfig weaponConfig;
        [SerializeField] public MovementBaseConfig movementBaseConfig;
    }
}