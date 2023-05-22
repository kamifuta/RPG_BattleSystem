using InGame.Skills;
using InGame.Parties;
using MyUtil;
using VContainer;
using InGame.Buttles.Actions;
using InGame.Items;
using UnityEngine;
using InGame.Magics;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAI
    {
        protected PartyManager partyManager;
        protected EnemyManager enemyManager;
        protected PlayableCharacterActionManager playableCharacterActionManager;

        [Inject]
        public PlayerAI(PartyManager partyManager)
        {
            this.partyManager = partyManager;
        }

        public void Init(EnemyManager enemyManager, PlayableCharacterActionManager playableCharacterActionManager)
        {
            this.enemyManager = enemyManager;
            this.playableCharacterActionManager = playableCharacterActionManager;
        }

        public virtual void SelectCharacterAction()
        {
            //子クラスで具体的な実装をする
        }
    }
}

