using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class BuffMagicMagic : MagicData
    {
        public override MagicType magicType => MagicType.BuffMagic;
        public override string magicName => "���̓A�b�v";
        public override string magicExplane => "���͂���i�K�㏸������";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseMagicBuffLevel(1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

