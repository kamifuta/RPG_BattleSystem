using InGame.Characters;
using InGame.Damages;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Magics
{
    public class MagicMissile : MagicData
    {
        public override MagicType magicType => MagicType.MagicMissile;
        public override string magicName => "�}�W�b�N�~�T�C��";
        public override string magicExplane => "���@�̒e�ōU������";
        public override int consumeMP => 15;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            var damage = new Damage(actor, actor.characterStatus.MagicValue, DamageTargetType.HP, AttackType.Magic, DamageAttributeType.None);
            target.ApplyDamage(damage);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

