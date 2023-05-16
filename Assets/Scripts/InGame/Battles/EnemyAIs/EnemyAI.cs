using InGame.Characters;
using InGame.Characters.Enemies;
using InGame.Damages;
using InGame.Parties;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InGame.Characters.Skills;
using InGame.Buttles.Actions;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAI : IDisposable
    {
        public EnemyCharacter targetEnemy { get; private set; }
        //private PartyManager partyManager;
        private EnemyAIMemory enemyAIMemory;

        private List<IDisposable> disposables = new List<IDisposable>();

        public EnemyAI(EnemyCharacter targetEnemy, PartyManager partyManager)
        {
            this.targetEnemy = targetEnemy;
            //this.partyManager = partyManager;

            enemyAIMemory = new EnemyAIMemory(partyManager.partyCharacters);

            ObserveAttacker();
        }

        public ActionData SelectAction()
        {
            //NOTE: ‚Æ‚è‚ ‚¦‚¸UŒ‚‚·‚é
            var target = enemyAIMemory.TargetPlayer;
            var arg = new ActionArgument(targetEnemy, target);
            return new ActionData(SkillType.NormalAttack, arg);
        }

        private void ObserveAttacker()
        {
            var disposable=targetEnemy.AttackerObservable
                .Where(x => x != null)
                .Subscribe(player =>
                {
                    enemyAIMemory.AddHate(player, 1);
                });

            disposables.Add(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}

