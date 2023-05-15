using InGame.Characters;
using InGame.Characters.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles
{
    public class ActionData
    {
        public SkillData skillData { get; private set; }

        private readonly BaseCharacter user;
        private readonly BaseCharacter target;

        public ActionData(SkillData skillData, BaseCharacter user, BaseCharacter target)
        {
            this.skillData = skillData;
            this.user = user;
            this.target = target;
        }

        public bool ExecuteAction()
        {
            if (target == null)
                return false;

            if (target.characterHealth.IsDead && !skillData.IsTargetableDeadCharacter)
                return false;

            skillData.ExecuteSkill(user, target);
            return true;
        }

        public bool ExecuteAction(BaseCharacter user, BaseCharacter target)
        {
            if (target == null)
                return false;

            if (target.characterHealth.IsDead && !skillData.IsTargetableDeadCharacter)
                return false;

            skillData.ExecuteSkill(user, target);
            return true;
        }
    }
}
