using InGame.Characters;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using System;

namespace InGame.Magics
{
    public abstract class MagicData
    {
        public abstract MagicType magicType { get; }
        public abstract string magicName { get; }
        public abstract string magicExplane { get; }
        public abstract int consumeMP { get; }
        public abstract TargetType targetType { get; }
        public abstract int priority { get; }
        public abstract bool IsTargetableDeadCharacter { get; }

        protected ISubject<Unit> pointlessActionSubject = new Subject<Unit>();
        public IObservable<Unit> PointlessActionObservable => pointlessActionSubject;

        public virtual void ExecuteMagic(BaseCharacter actor, BaseCharacter target)
        {
            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        public virtual void ExecuteMagic(BaseCharacter actor, IEnumerable<BaseCharacter> targets)
        {
            if (!IsTargetableAnyCharacter)
            {
                if (targets.Count() == 1)
                {
                    ExecuteMagic(actor, targets.Single());
                }
                else
                {
                    Debug.LogError($"{magicName}�͕����̃L�������^�[�Q�b�g�ɂ��邱�Ƃ͂ł��܂���");
                }
            }

            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        protected bool IsTargetableAnyCharacter
            => targetType == TargetType.AllEnemy || targetType == TargetType.AllFriends;
    }
}

