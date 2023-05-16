using InGame.Characters;
using InGame.Characters.Skills;
using InGame.Items;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public class ActionData
    {
        public SkillData skillData { get; private set; }

        //private readonly BaseCharacter user;
        //private readonly BaseCharacter target;

        private readonly ActionArgument actionArgument;

        //public ActionData(SkillData skillData, BaseCharacter user, BaseCharacter target)
        //{
        //    this.skillData = skillData;
        //    this.user = user;
        //    this.target = target;
        //}

        public ActionData(SkillType skillType, ActionArgument actionArgument)
        {
            this.skillData = SkillDataBase.GetSkillData(skillType);
            this.actionArgument = actionArgument;

            if (skillType == SkillType.UseItem)
            {
                var arg = actionArgument as UseItemActionArgument;
                var item = ItemDataBase.GetItemData(arg.useItemType);
                skillData.SetTargetType(item.targetType);
                skillData.SetIsTargetableDeadCharacter(item.IsTargetableDeadCharacter);
            }
        }

        public bool ExecuteAction()
        {
            if (actionArgument == null)
                return false;

            if (actionArgument.targets.Count() == 1)
            {
                var target = actionArgument.targets.Single();
                if (target.characterHealth.IsDead && !skillData.IsTargetableDeadCharacter)
                    return false;
            }
            
            skillData.ExecuteSkill(actionArgument);
            return true;
        }

        public bool ExecuteAction(ActionArgument actionArgument)
        {
            if (actionArgument == null)
                return false;

            if (actionArgument.targets.Count() == 1)
            {
                var target = actionArgument.targets.Single();
                if (target.characterHealth.IsDead && !skillData.IsTargetableDeadCharacter)
                    return false;
            }

            skillData.ExecuteSkill(actionArgument);
            return true;
        }
    }
}
