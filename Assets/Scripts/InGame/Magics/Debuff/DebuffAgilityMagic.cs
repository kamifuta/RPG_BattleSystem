using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class DebuffAgilityMagic : MagicData
    {
        public override MagicType magicType => MagicType.DebuffAgility;
        public override string magicName => "素早さ下げ";
        public override string magicExplane => "素早さを一段階減少させる";
        public override int consumeMP => 10;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseAgilityBuffLevel(-1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

