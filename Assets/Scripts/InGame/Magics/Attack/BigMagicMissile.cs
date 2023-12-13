using InGame.Characters;
using InGame.Damages;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Magics
{
    public class BigMagicMissile : MagicData
    {
        public override MagicType magicType => MagicType.BigMagicMissile;
        public override string magicName => "ビッグマジックミサイル";
        public override string magicExplane => "巨大な魔法の弾で攻撃する";
        public override int consumeMP => 15;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        private const float MagicMagnification = 1.2f;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            var magicValue = Mathf.CeilToInt(actor.characterStatus.MagicValue * MagicMagnification);
            var damage = new Damage(actor, magicValue, DamageTargetType.HP, AttackType.Magic, DamageAttributeType.None);
            target.ApplyDamage(damage);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

