using InGame.Buttles;
using InGame.Buttles.PlayerAIs;
using InGame.Fields;
using InGame.Parties;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Containers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private EncountView encountView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BattleController>();
            builder.RegisterEntryPoint<EncountPresenter>();

            builder.Register<FieldManager>(Lifetime.Singleton);
            builder.Register<PartyManager>(Lifetime.Singleton);
            builder.Register<PlayerAI>(Lifetime.Singleton);

            builder.RegisterComponent(encountView);
        }
    }
}

