using InGame.Characters;
using InGame.Characters.Enemies;
using InGame.Damages;
using InGame.Parties;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAI
    {
        public EnemyCharacter targetEnemy { get; private set; }
        private PartyManager partyManager;

        public EnemyAI(EnemyCharacter targetEnemy, PartyManager partyManager)
        {
            this.targetEnemy = targetEnemy;
            this.partyManager = partyManager;
        }

        public ActionInfo SelectAction()
        {
            //NOTE: Ç∆ÇËÇ†Ç¶Ç∏çUåÇÇ∑ÇÈ
            var target = partyManager.partyCharacters.RandomGet();
            return new ActionInfo(targetEnemy.Attack, target, TargetType.Enemy);
        }
    }
}

