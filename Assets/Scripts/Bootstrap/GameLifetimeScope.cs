using Configs;
using Features.AI;
using Features.Combat;
using Features.GameSave;
using Features.Movement;
using Features.Player;
using Features.PowerUps;
using Features.Score;
using Features.Spawning;
using UI.Menu;
using UI.Score;
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
            builder.RegisterComponentInHierarchy<ScorePopupView>().As<IScorePopupService>();
            builder.Register<ScoreService>(Lifetime.Singleton).As<IScoreService>();
            builder.Register<IAIController, SimpleAI>(Lifetime.Transient);
            builder.Register<IWeaponFactory, WeaponFactory>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<KeyboardMouseTankInput>().As<ITankInputSource>();
            builder.Register<IPlayerControllerFactory, PlayerControllerFactory>(Lifetime.Singleton);
            builder.Register<IPlayerController, PlayerController>(Lifetime.Transient);
            builder.RegisterComponentInHierarchy<BattlefieldSpawner>();
            builder.RegisterComponentInHierarchy<PowerUpSpawner>();
            builder.Register<IGameSaveService, GameSaveService>(Lifetime.Transient);
            
            builder.RegisterComponentInHierarchy<GameMenusController>();    
            builder.RegisterComponentInHierarchy<MainMenuUI>().AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<GameMenuUI>().AsSelf().AsImplementedInterfaces();
            builder.RegisterEntryPoint<GameLoop>();
        }
    }
    
    public sealed class GameLoop : ITickable
    {
        public void Tick() { }
    }
}
