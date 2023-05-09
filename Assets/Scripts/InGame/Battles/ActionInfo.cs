using InGame.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles
{
    public enum TargetType
    {
        Self,
        Friends,
        Enemy,
    }

    public class ActionInfo
    {
        public Action<BaseCharacter> action { get; private set; }
        private BaseCharacter target;
        public TargetType targetType { get; private set; }
        public int priority { get; private set; }
        private bool IsTargetableDeadCharacter;

        public ActionInfo(Action<BaseCharacter> action, BaseCharacter target, TargetType targetType, int priority=0, bool IsTargetableDeadCharacter=false)
        {
            this.action = action;
            this.target = target;
            this.targetType = targetType;
            this.priority = priority;
            this.IsTargetableDeadCharacter = IsTargetableDeadCharacter;
        }

        public bool ExecuteAction()
        {
            if (target == null)
                return false;

            if (target.characterHealth.IsDead && !IsTargetableDeadCharacter)
                return false;

            action?.Invoke(target);
            return true;
        }
    }
}

