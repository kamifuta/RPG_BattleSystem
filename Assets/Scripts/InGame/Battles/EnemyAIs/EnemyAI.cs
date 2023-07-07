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
using InGame.Skills;
using InGame.Buttles.Actions;
using InGame.Characters.PlayableCharacters;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAI : IDisposable
    {
        public EnemyCharacter targetEnemy { get; private set; }
        private EnemyAIMemory enemyAIMemory;

        private List<IDisposable> disposables = new List<IDisposable>();

        public EnemyAI(EnemyCharacter targetEnemy, IEnumerable<PlayableCharacter> playableCharacters)
        {
            this.targetEnemy = targetEnemy;

            enemyAIMemory = new EnemyAIMemory(playableCharacters);

            ObserveAttacker();
        }

        public ActionData SelectAction()
        {
            //NOTE: ‚Æ‚è‚ ‚¦‚¸UŒ‚‚·‚é
            var target = enemyAIMemory.TargetPlayer();
            return new ActionData(BaseActionType.NormalAttack, targetEnemy, target);
        }

        private void ObserveAttacker()
        {
            var disposable=targetEnemy.AttackerObservable
                .Where(x => x.Item1 != null)
                .Subscribe(damage =>
                {
                    var rate = (float)damage.Item2 / targetEnemy.characterStatus.MaxHP;
                    enemyAIMemory.AddHate(damage.Item1, rate);
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

