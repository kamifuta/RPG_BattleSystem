using InGame.Agents;
using InGame.Agents.Players;
using InGame.Buttles;
using InGame.Buttles.PlayerAIs;
using InGame.Characters;
using InGame.Fields;
using InGame.Parties;
using Unity.MLAgents;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Containers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private EncountView encountView;
        [SerializeField] private PlayerAgent playerAgent;

        [SerializeField] private EnemyStatusDataTable enemyStatusDataTable;
        [SerializeField] private PlayableCharacterStatusDataTable playerStatusDataTable;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BattleController>();
            builder.RegisterEntryPoint<EncountPresenter>();

            builder.Register<FieldManager>(Lifetime.Singleton);
            builder.Register<PartyManager>(Lifetime.Singleton).WithParameter("statusDataTable", playerStatusDataTable);
            builder.Register<PlayerAI, NormalAttackAndDefenceAI>(Lifetime.Singleton);
            builder.Register<RewardProvider>(Lifetime.Scoped);

            builder.Register<EnemyFactory>(Lifetime.Transient).WithParameter("enemyStatusDataTable", enemyStatusDataTable);

            builder.RegisterComponent(encountView);
            builder.RegisterComponent(playerAgent);
        }
    }
}

