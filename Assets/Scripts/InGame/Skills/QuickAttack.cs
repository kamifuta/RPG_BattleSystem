using InGame.Characters;
using InGame.Damages;
using UnityEngine;

namespace InGame.Skills
{
    public class QuickAttack : SkillData
    {
        public override SkillType skillType => SkillType.QuickAttack;
        public override string skillName => "Ž¾•—UŒ‚";
        public override string skillExplane => "•K‚¸æ§‚ÅUŒ‚‚·‚é";
        public override int consumeMP => 5;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 1;
        public override bool IsTargetableDeadCharacter => false;

        private const float attackMagnification = 0.9f;

        public override void ExecuteSkill(BaseCharacter actor, BaseCharacter target)
        {
            var attackValue = Mathf.CeilToInt(actor.characterStatus.AttackValue * attackMagnification);
            var damage = new Damage(actor, attackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None);
            target.ApplyDamage(damage);
        }
    }
}

