using Configs;
using Features.AI;
using Features.Combat;
using Features.Movement;
using Features.Player;
using Features.Spawning;
using MainMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BattlefieldConfig battlefieldConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            if (battlefieldConfig != null)
            {
                builder.RegisterInstance(battlefieldConfig);
            }

            builder.Register<IMovementController, SteeringMovement>(Lifetime.Transient);
            builder.Register<IAIController, SimpleAI>(Lifetime.Transient);
            builder.Register<IWeaponFactory, WeaponFactory>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<KeyboardMouseTankInput>().As<ITankInputSource>();
            builder.Register<IPlayerControllerFactory, PlayerControllerFactory>(Lifetime.Singleton);
            builder.Register<IPlayerController, PlayerController>(Lifetime.Transient);
            builder.RegisterComponentInHierarchy<BattlefieldSpawner>();
            builder.RegisterComponentInHierarchy<MainMenuUI>();
            
            builder.Register<RespawnManager>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameLoop>(Lifetime.Singleton);
        }
    }
    
    public sealed class GameLoop : ITickable
    {
        public void Tick() { }
    }
}
