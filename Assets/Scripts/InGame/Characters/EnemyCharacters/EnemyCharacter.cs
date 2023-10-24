using InGame.Characters.PlayableCharacters;
using InGame.Damages;
using InGame.Magics;
using InGame.Skills;
using Log;
using System;
using System.Collections.Generic;
using UniRx;

namespace InGame.Characters.Enemies
{
    public class EnemyCharacter : BaseCharacter
    {
        private readonly ISubject<PlayableCharacter> attackerSubject = new Subject<PlayableCharacter>();
        public IObservable<PlayableCharacter> AttackerObservable => attackerSubject;

        public EnemyCharacter(CharacterStatus characterStatus, List<SkillType> rememberSkillList, List<MagicType> rememberMagicList) : base(characterStatus)
        {
            rememberSkills = rememberSkillList;
            rememberMagics = rememberMagicList;
        }

        public override void ApplyDamage(Damage damage)
        {
            var damageValue = DamageCalculator.CalcDamage(damage, this);
            characterHealth.ApplyDamage(damageValue);
            attackerSubject.OnNext(damage.attacker as PlayableCharacter);
            //LogWriter.WriteLog($"{characterName}Ç…{damageValue.ToString()}ÇÃÉ_ÉÅÅ[ÉW");

            //if (characterHealth.IsDead)
                //LogWriter.WriteLog($"{characterName}ÇÕì|ÇÍÇΩ");
        }
    }
}

