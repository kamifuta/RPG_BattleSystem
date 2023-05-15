using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters.Skills
{
    public enum TargetType
    {
        Self,
        Friends,
        Enemy,
    }

    public class SkillData
    {
        public SkillType skillType { get; private set; }
        public string skillName { get; private set; }
        public string skillExplane { get; private set; }
        public int consumeMP { get; private set; }
        public TargetType targetType { get; private set; }
        public int priority { get; private set; }
        public bool IsTargetableDeadCharacter { get; private set; }
        public Action<BaseCharacter, BaseCharacter, int> skillFunction { get; private set; }

        public SkillData(SkillType skillType, string skillName, string skillExplane, int consumeMP, TargetType targetType, int priority,
            bool IsTargetableDeadCharacter, Action<BaseCharacter, BaseCharacter, int> skillFunction)
        {
            this.skillType = skillType;
            this.skillName = skillName;
            this.skillExplane = skillExplane;
            this.consumeMP = consumeMP;
            this.targetType = targetType;
            this.priority = priority;
            this.IsTargetableDeadCharacter = IsTargetableDeadCharacter;
            this.skillFunction = skillFunction;
        }

        public void ExecuteSkill(BaseCharacter user, BaseCharacter target)
        {
            skillFunction?.Invoke(user, target, consumeMP);
        }
    }
}

