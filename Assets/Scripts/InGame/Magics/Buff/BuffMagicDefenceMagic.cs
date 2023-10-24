using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class BuffMagicDefenceMagic : MagicData
    {
        public override MagicType magicType => MagicType.BuffMagicDefence;
        public override string magicName => "���h�A�b�v";
        public override string magicExplane => "���@�h��͂���i�K�㏸������";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseMagicDefenceBuffLevel(1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

