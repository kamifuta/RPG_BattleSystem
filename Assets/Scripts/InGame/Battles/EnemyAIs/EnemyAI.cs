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
            //NOTE: �Ƃ肠�����U������
            var target = enemyAIMemory.TargetPlayer;
            return new ActionData(SkillDataBase.GetSkillData(SkillType.NormalAttack),targetEnemy, target);
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

