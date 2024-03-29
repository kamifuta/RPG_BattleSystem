using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class BuffAgilityMagic : MagicData
    {
        public override MagicType magicType => MagicType.BuffAgility;
        public override string magicName => "ピオラ";
        public override string magicExplane => "素早さを一段階上昇させる";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseAgilityBuffLevel(1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

