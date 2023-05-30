using InGame.Buttles.Actions;
using InGame.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Skills
{
    public enum TargetType
    {
        Self,
        Friends,
        AllFriends,
        Enemy,
        AllEnemy,
    }

    public abstract class SkillData
    {
        public abstract SkillType skillType { get; }
        public abstract string skillName { get; }
        public abstract string skillExplane { get; }
        public abstract int consumeMP { get; }
        public abstract TargetType targetType { get; }
        public abstract int priority { get; }
        //public abstract bool IsTargetableDeadCharacter { get; }

        public virtual void ExecuteSkill(BaseCharacter actor, BaseCharacter target)
        {
            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        public virtual void ExecuteSkill(BaseCharacter actor, IEnumerable<BaseCharacter> targets)
        {
            if (!IsTargetableAnyCharacter)
            {
                if (targets.Count() == 1)
                {
                    ExecuteSkill(actor, targets.Single());
                }
                else
                {
                    Debug.LogError($"{skillName}�͕����̃L�������^�[�Q�b�g�ɂ��邱�Ƃ͂ł��܂���");
                }
            }

            //��̓I�ȓ��e�͎q�N���X�Ŏ���
        }

        protected bool IsTargetableAnyCharacter
            => targetType == TargetType.AllEnemy || targetType == TargetType.AllFriends;
    }
}

