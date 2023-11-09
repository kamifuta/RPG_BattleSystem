using InGame.Characters;
using InGame.Damages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Skills
{
    public class DefenceDownAttack : SkillData
    {
        public override SkillType skillType => SkillType.DefenceDownAttack;
        public override string skillName => "–hŒä‰º‚°UŒ‚";
        public override string skillExplane => "UŒ‚‚µ‚½‘ÎÛ‚Ì•¨—–hŒä‚ðŠm—¦‚Å‰º‚°‚é";
        public override int consumeMP => 10;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => false;

        private const float AttackMagnification = 1.1f;

        public override void ExecuteSkill(BaseCharacter actor, BaseCharacter target)
        {
            var attackValue = Mathf.CeilToInt(actor.characterStatus.AttackValue * AttackMagnification);
            var damage = new Damage(actor, attackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None);
            target.ApplyDamage(damage);

            if (Random.value < 0.3f)
            {
                target.characterStatus.characterBuff.RaiseDefenceBuffLevel(-1);
            }

            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

