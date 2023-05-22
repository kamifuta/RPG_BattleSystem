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
        public override string magicName => "";
        public override string magicExplane => "";
        public override int consumeMP => 50;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => true;

        private const float SuccessRate = 0.5f;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            var rand = Random.value;
            if (rand <= SuccessRate)
            {
                target.Revaival();
            }
            else
            {
                LogWriter.WriteLog("•œŠˆ‚ÉŽ¸”s‚µ‚½");
            }
            
        }
    }
}

