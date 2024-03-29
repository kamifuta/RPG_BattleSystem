using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class DebuffDefenceMagic : MagicData
    {
        public override MagicType magicType => MagicType.DebuffDefence;
        public override string magicName => "防御下げ";
        public override string magicExplane => "物理防御力を一段階減少させる";
        public override int consumeMP => 10;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseDefenceBuffLevel(-1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

