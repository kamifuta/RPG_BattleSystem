using InGame.Characters;
using InGame.Skills;
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
        //public abstract bool IsTargetableDeadCharacter { get; }

        public virtual void ExecuteEffect(BaseCharacter target)
        {
            //具体的な内容は子クラスで実装
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
                    Debug.LogError($"{itemName}は複数のキャラをターゲットにすることはできません");
                }
            }

            //具体的な内容は子クラスで実装
        }

        protected bool IsTargetableAnyCharacter
            => targetType == TargetType.AllEnemy || targetType == TargetType.AllFriends;
    }
}

