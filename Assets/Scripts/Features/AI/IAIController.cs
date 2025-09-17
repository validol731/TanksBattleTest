using Configs;
using Features.AI.Config;
using Features.Movement;

namespace Features.AI
{
    public interface IAIController
    {
        void Setup(IMovementController move, BattlefieldConfig cfg, UnityEngine.Rigidbody2D rb);
        void Tick(float dt);
        void OnCollision();
        void ApplyProfile(AITankConfig config);
    }
}