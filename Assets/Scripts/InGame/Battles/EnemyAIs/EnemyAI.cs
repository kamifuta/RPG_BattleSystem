using InGame.Characters.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using InGame.Buttles.Actions;
using InGame.Characters.PlayableCharacters;
using InGame.Magics;
using MyUtil;
using InGame.Skills;
using System.Linq;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAI : IDisposable
    {
        public EnemyCharacter targetEnemy { get; private set; }
        private readonly EnemyAIMemory enemyAIMemory;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public EnemyAI(EnemyCharacter targetEnemy, IEnumerable<PlayableCharacter> playableCharacters)
        {
            this.targetEnemy = targetEnemy;

            enemyAIMemory = new EnemyAIMemory(playableCharacters);

            ObserveDamage();
        }

        private void ObserveDamage()
        {
            var disposable = targetEnemy.AttackerObservable.Zip(targetEnemy.characterHealth.DamagedValueObservable, (attacker, damageValue) => (attacker, damageValue))
                .Where(x => x.attacker != null)
                .Subscribe(damage =>
                {
                    var rate = (float)damage.damageValue / targetEnemy.characterStatus.MaxHP;
                    enemyAIMemory.AddHate(damage.attacker, rate);
                });

            disposables.Add(disposable);
        }

        public ActionData SelectAction()
        {
            if (targetEnemy.HPRate<0.3f)
            {
                return SelectAttackOrHeal();
            }
            else
            {
                return SelectAttackSkill();
            }
        }

        private ActionData SelectAttackOrHeal()
        {
            var rand = new Random();

            if (rand.NextDouble() < 0.2f && MagicDataBase.GetMagicData(MagicType.HealMagic).consumeMP<targetEnemy.characterMagic.currentMP)
            {
                return new ActionData(BaseActionType.UseMagic, targetEnemy, targetEnemy, MagicType.HealMagic);
            }
            else
            {
                return SelectAttackSkill();
            }
        }

        private ActionData SelectAttackSkill()
        {
            var rand = new Random();
            var target = enemyAIMemory.TargetPlayer();

            if (rand.NextDouble() < 0.4f)
            {
                return new ActionData(BaseActionType.NormalAttack, targetEnemy, target);
            }
            else 
            {
                var usableSkillList = targetEnemy.rememberSkills.Where(x => SkillDataBase.GetSkillData(x).consumeMP < targetEnemy.characterMagic.currentMP);
                if (!usableSkillList.Any())
                {
                    return new ActionData(BaseActionType.NormalAttack, targetEnemy, target);
                }

                var skillType = usableSkillList.RandomGet();
                var skillData = SkillDataBase.GetSkillData(skillType);
                switch (skillData.targetType)
                {
                    case TargetType.Enemy:
                        return new ActionData(BaseActionType.UseSkill, targetEnemy, target, skillType);
                    case TargetType.AllEnemy:
                        return new ActionData(BaseActionType.UseSkill, targetEnemy, enemyAIMemory.HateCharacters, skillType);
                    default:
                        return null;
                }
                
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}

