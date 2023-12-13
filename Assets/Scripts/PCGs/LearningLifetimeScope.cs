using InGame.Agents.Players;
using InGame.Buttles;
using InGame.Characters;
using InGame.Fields;
using PCGs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LearningLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerAgentFactory playerAgentFactory;

    [SerializeField] private EnemyStatusDataTable enemyStatusDataTable;

    [SerializeField] private CharacterStatusData PCGStatusData;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<Learning>();

        builder.Register<FieldManager>(Lifetime.Singleton);
        builder.Register<CharacterManager>(Lifetime.Scoped).WithParameter("characterStatusData", PCGStatusData);

        builder.Register<EnemyFactory>(Lifetime.Transient)
            .WithParameter("enemyStatusDataTable", enemyStatusDataTable);

        builder.RegisterComponent(playerAgentFactory);
    }
}
