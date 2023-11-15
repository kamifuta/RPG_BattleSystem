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
    public class AnalyzeLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerAgentFactory playerAgentFactory;

        [SerializeField] private EnemyStatusDataTable enemyStatusDataTable;

        [SerializeField] private CharacterStatusData PCGStatusData;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CharacterAnalyser>().WithParameter("characterStatusData", PCGStatusData);

            builder.Register<FieldManager>(Lifetime.Singleton);
            builder.Register<CharacterManager>(Lifetime.Scoped).WithParameter("characterStatusData", PCGStatusData);

            builder.Register<EnemyFactory>(Lifetime.Transient)
                .WithParameter("enemyStatusDataTable", enemyStatusDataTable);

            builder.RegisterComponent(playerAgentFactory);
        }
    }
}

