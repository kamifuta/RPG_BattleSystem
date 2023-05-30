using InGame.Characters;
using InGame.Damages;
using UnityEngine;

namespace InGame.Skills
{
    public class PowerAttack : SkillData
    {
        public override SkillType skillType => SkillType.PowerAttack;
        public override string skillName => "���U��";
        public override string skillExplane => "�ʏ��苭�͂ȍU�����s��";
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
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

