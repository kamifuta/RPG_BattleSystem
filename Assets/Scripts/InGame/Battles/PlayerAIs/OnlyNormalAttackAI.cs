using InGame.Buttles.Actions;
using InGame.Parties;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace InGame.Buttles.PlayerAIs
{
    public class OnlyNormalAttackAI : PlayerAI
    {
        [Inject]
        public OnlyNormalAttackAI(PartyManager partyManager) : base(partyManager)
        {

        }

        public override void SelectCharacterAction()
        {
            foreach(var player in partyManager.partyCharacters)
            {
                var target = enemyManager.enemies.RandomGet();
                var action = new ActionData(BaseActionType.NormalAttack, player, target);
                playableCharacterActionManager.SetPlayableCharacterAction(player, action);
            }
        }
    }
}

