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
using VContainer;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAI
    {
        private PartyManager partyManager;
        private EnemyManager enemyManager;
        private PlayableCharacterActionManager playableCharacterActionManager;

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

        public void SelectCharacterAction()
        {
            foreach(var character in partyManager.partyCharacters)
            {
                if (character.characterHealth.IsDead)
                    continue;

                var random = UnityEngine.Random.value;
                if (random < 0.1f)
                {
                    var target = enemyManager.enemies.RandomGet();
                    ActionInfo action = new ActionInfo(character.Attack, target, TargetType.Enemy);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                else
                {
                    ActionInfo action = new ActionInfo(character.Defence, character, TargetType.Self, priority: 1);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }

                
            }
        }
    }
}

