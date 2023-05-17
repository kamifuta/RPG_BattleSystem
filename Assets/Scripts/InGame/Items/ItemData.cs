using InGame.Characters;
using InGame.Characters.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InGame.Items
{
    public abstract class ItemData
    {
        public abstract string itemName { get; }
        public abstract string itemExplane { get; }
        public abstract ItemType itemType { get; }
        public abstract TargetType targetType { get; }
        public abstract bool IsTargetableDeadCharacter { get; }

        public virtual void ExecuteEffect(BaseCharacter target)
        {
            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        public virtual void ExecuteEffect(IEnumerable<BaseCharacter> targets)
        {
            if (!IsTargetableAnyCharacter)
            {
                if (targets.Count() == 1)
                {
                    ExecuteEffect(targets.Single());
                }
                else
                {
                    Debug.LogError($"{itemName}�͕����̃L�������^�[�Q�b�g�ɂ��邱�Ƃ͂ł��܂���");
                }
            }

            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        protected bool IsTargetableAnyCharacter
            => targetType == TargetType.AllEnemy || targetType == TargetType.AllFriends;
    }
}

