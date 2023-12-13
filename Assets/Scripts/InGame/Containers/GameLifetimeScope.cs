using InGame.Agents;
using InGame.Agents.Players;
using InGame.Buttles;
using InGame.Characters;
using InGame.Fields;
using InGame.Parties;
using PCGs;
using Unity.MLAgents;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Containers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerAgentFactory playerAgentFactory;

        [SerializeField] private PCGSettings PCGSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ParameterSearcher>().WithParameter("PCGSettings", PCGSettings);

            builder.Register<FieldManager>(Lifetime.Singleton);
            builder.Register<CharacterManager>(Lifetime.Scoped).WithParameter("characterStatusData", PCGSettings.StatusData);

            builder.Register<EnemyFactory>(Lifetime.Transient)
                .WithParameter("enemyStatusDataTable", PCGSettings.EnemyStatusDataTable);

            builder.RegisterComponent(playerAgentFactory);
        }
    }
}

