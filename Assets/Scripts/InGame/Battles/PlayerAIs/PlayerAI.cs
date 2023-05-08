using InGame.Buttles;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Parties;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAI
    {
        private PartyManager partyManager;
        private EnemyManager enemyManager;
        private PlayableCharacterActionManager PlayableCharacterActionManager;
        private TurnManager turnManager;

        public void ObserveTurn()
        {
            turnManager.CurrentTurn
                .Where(turn => turn == TurnType.ActionSelect)
                .Subscribe(_ =>
                {
                    SelectCharacterAction();
                });
        }

        public void SelectCharacterAction()
        {
            foreach(var character in partyManager.partyCharacters)
            {
                var target = enemyManager.enemies.RandomGet();
                Action<BaseCharacter> action = _ => character.Attack(target);
                PlayableCharacterActionManager.SetPlayableCharacterAction(character, action);
            }
        }
    }
}

