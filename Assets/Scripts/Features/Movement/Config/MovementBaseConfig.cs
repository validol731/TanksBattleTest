using UnityEngine;

namespace Features.Movement.Config
{
    [CreateAssetMenu(menuName = "Configs/Movement/MovementBaseConfig")]
    public class MovementBaseConfig: ScriptableObject
    {
        [SerializeField] public float maxForwardSpeed = 4f;
        [SerializeField] public float maxBackwardSpeed = 3f;
        [SerializeField] public float acceleration = 10f;
        [SerializeField] public float turnRadius = 2.0f;
        [SerializeField] public float omegaInPlace = 2.5f;
        [SerializeField] public float omegaMaxClamp = 2.5f;
    }
}