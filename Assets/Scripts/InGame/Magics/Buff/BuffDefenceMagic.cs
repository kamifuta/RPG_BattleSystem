using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class BuffDefenceMagic : MagicData
    {
        public override MagicType magicType => MagicType.BuffDefence;
        public override string magicName => "スカラ";
        public override string magicExplane => "物理防御力を一段階上昇させる";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseDefenceBuffLevel(1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

