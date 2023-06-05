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
        public override string magicName => "�q�[��";
        public override string magicExplane => "�̗͂��񕜂�����";
        public override int consumeMP => 20;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        //public override bool IsTargetableDeadCharacter => false;

        private const int lowestHealingValue = 30;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            if (target.characterHealth.IsDead)
            {
                LogWriter.WriteLog("�����������N����Ȃ�����");
                pointlessActionSubject.OnNext(Unit.Default);
                return;
            }

            var baseHealingValue = lowestHealingValue + (lowestHealingValue * (actor.characterStatus.MagicValue * 0.16f));
            var healingValue = Mathf.CeilToInt(baseHealingValue + baseHealingValue * Random.Range(0.05f, 0.05f));
            var heal = new Healing(healingValue, 0);
            target.Heal(heal);
            actor.characterMagic.DecreaseMP(consumeMP);
        }
    }
}

