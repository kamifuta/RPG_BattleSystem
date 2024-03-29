using InGame.Characters;
using InGame.Healings;
using InGame.Skills;
using Log;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Magics
{
    public class HealMagic : MagicData
    {
        public override MagicType magicType => MagicType.HealMagic;
        public override string magicName => "ヒール";
        public override string magicExplane => "体力を回復させる";
        public override int consumeMP => 15;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => false;

        private const int lowestHealingValue = 60;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            if(target.characterHealth.IsDead || target.HPRate == 1)
            {
                pointlessActionSubject.OnNext(Unit.Default);
                return;
            }

            var baseHealingValue = lowestHealingValue + (actor.characterStatus.MagicValue * 0.45f);
            var healingValue = Mathf.CeilToInt(baseHealingValue + baseHealingValue * Random.Range(0.05f, 0.05f));
            var heal = new Healing(healingValue, 0);
            target.Heal(heal);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

