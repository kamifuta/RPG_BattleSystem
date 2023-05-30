using InGame.Characters;
using InGame.Healings;
using InGame.Skills;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Magics
{
    public class RevivalMagic : MagicData
    {
        public override MagicType magicType => MagicType.RevivalMagic;
        public override string magicName => "リバイバル";
        public override string magicExplane => "確率で蘇生させる";
        public override int consumeMP => 50;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => true;

        private const float SuccessRate = 0.5f;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            if (!target.characterHealth.IsDead)
            {
                LogWriter.WriteLog("しかし何も起こらなかった");
                return;
            }

            var rand = Random.value;
            if (rand <= SuccessRate)
            {
                target.Revaival();
            }
            else
            {
                LogWriter.WriteLog("復活に失敗した");
            }
            actor.characterMagic.DecreaseMP(consumeMP);

        }
    }
}

