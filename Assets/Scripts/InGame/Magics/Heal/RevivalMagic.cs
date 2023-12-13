using InGame.Characters;
using InGame.Healings;
using InGame.Skills;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace InGame.Magics
{
    public class RevivalMagic : MagicData
    {
        public override MagicType magicType => MagicType.RevivalMagic;
        public override string magicName => "���o�C�o��";
        public override string magicExplane => "�m���őh��������";
        public override int consumeMP => 25;
        public override TargetType targetType => TargetType.Friends;
        public override int priority => 0;
        public override bool IsTargetableDeadCharacter => true;

        private const float SuccessRate = 0.5f;

        public override void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            if (!target.characterHealth.IsDead)
            {
                //LogWriter.WriteLog("�����������N����Ȃ�����");
                pointlessActionSubject.OnNext(Unit.Default);
                return;
            }

            var rand = Random.value;
            if (rand <= SuccessRate)
            {
                target.Revaival();
            }
            else
            {
                //LogWriter.WriteLog("�����Ɏ��s����");
            }
            actor.characterMagic.DecreaseMP(consumeMP);

        }
    }
}

