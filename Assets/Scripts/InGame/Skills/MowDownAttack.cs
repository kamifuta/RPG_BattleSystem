using InGame.Characters;
using InGame.Damages;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Skills
{
    public class MowDownAttack : SkillData
    {
        public override SkillType skillType => SkillType.MowDownAttack;
        public override string skillName => "“ã‚¬•¥‚¢UŒ‚";
        public override string skillExplane => "‘S‘Ì‚ÉUŒ‚‚ðs‚¤";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.AllEnemy;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => false;

        private const float AttackMagnification = 0.75f;

        public override void ExecuteSkill(BaseCharacter actor, IEnumerable<BaseCharacter> targets)
        {
            var attackValue = Mathf.CeilToInt(actor.characterStatus.AttackValue * AttackMagnification);
            var damage = new Damage(actor, attackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None);
            targets.ForEach(x=>x.ApplyDamage(damage));
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

