using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class DebuffMagicDefenceMagic : MagicData
    {
        public override MagicType magicType => MagicType.DebuffMagicDefence;
        public override string magicName => "���h�_�E��";
        public override string magicExplane => "���@�h��͂���i�K����������";
        public override int consumeMP => 10;
        public override TargetType targetType => TargetType.Enemy;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            target.characterStatus.characterBuff.RaiseMagicDefenceBuffLevel(-1);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

