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
        public override string magicName => "�f��������";
        public override string magicExplane => "�f��������i�K����������";
        public override int consumeMP => 20;
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

