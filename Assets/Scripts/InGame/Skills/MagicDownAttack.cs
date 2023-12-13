using InGame.Characters;
using InGame.Damages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Skills
{
    public class MagicDownAttack : SkillData
    {
        public override SkillType skillType => SkillType.MagicDownAttack;
        public override string skillName => "–‚—Í‰º‚°UŒ‚";
        public override string skillExplane => "UŒ‚‚µ‚½‘ÎÛ‚Ì–‚—Í‚ðŠm—¦‚Å‰º‚°‚é";
        public override int consumeMP => 15;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => false;

        private const float AttackMagnification = 1.05f;
        private const float AddEffectRate = 0.1f;

        public override void ExecuteSkill(BaseCharacter actor, BaseCharacter target)
        {
            var attackValue = Mathf.CeilToInt(actor.characterStatus.AttackValue * AttackMagnification);
            var damage = new Damage(actor, attackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None);
            target.ApplyDamage(damage);

            if (Random.value < AddEffectRate)
            {
                target.characterStatus.characterBuff.RaiseMagicBuffLevel(-1);
            }

            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

