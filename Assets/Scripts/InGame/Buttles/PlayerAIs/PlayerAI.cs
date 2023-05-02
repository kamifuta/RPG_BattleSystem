using InGame.Buttles;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Parties;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAI
    {
        private PartyManager partyManager;
        private EnemyManager enemyManager;
        private PlayableCharacterActionManager PlayableCharacterActionManager;

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

