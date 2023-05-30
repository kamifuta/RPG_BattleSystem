using InGame.Characters;
using InGame.Skills;
using InGame.Items;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InGame.Magics;

namespace InGame.Buttles.Actions
{
    public class ActionData
    {
        public BaseActionType actionType;

        private readonly BaseCharacter actor;
        private readonly BaseCharacter target;
        public readonly ItemType itemType;
        public readonly SkillType skillType;
        public readonly MagicType magicType;

        private readonly ActionArgument actionArgument;

        public ActionData(BaseActionType actionType, BaseCharacter actor)
        {
            this.actionType = actionType;
            this.actor = actor;
        }

        public ActionData(BaseActionType actionType, BaseCharacter actor, BaseCharacter target)
        {
            this.actionType = actionType;
            this.actor = actor;
            this.target = target;
        }

        public ActionData(BaseActionType actionType, BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            this.actionType = actionType;
            this.actor = actor;
            this.target = target;
            this.itemType = itemType;
        }

        public ActionData(BaseActionType actionType, BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            this.actionType = actionType;
            this.actor = actor;
            this.target = target;
            this.skillType = skillType;
        }

        public ActionData(BaseActionType actionType, BaseCharacter actor, BaseCharacter target, MagicType magicType)
        {
            this.actionType = actionType;
            this.actor = actor;
            this.target = target;
            this.magicType = magicType;
        }

        public bool ExecuteAction()
        {
            switch (actionType)
            {
                case BaseActionType.NormalAttack:
                    if (target.characterHealth.IsDead)
                        return false;
                    BaseActionFunctions.NormalAttack(actor, target);
                    break;
                case BaseActionType.Defence:
                    BaseActionFunctions.Defence(actor);
                    break;
                case BaseActionType.UseItem:
                    var item = ItemDataBase.GetItemData(itemType);
                    BaseActionFunctions.UseItem(actor, target, itemType);
                    break;
                case BaseActionType.UseSkill:
                    var skill = SkillDataBase.GetSkillData(skillType);
                    BaseActionFunctions.UseSkill(actor, target, skillType);
                    break;
                case BaseActionType.UseMagic:
                    var magic = MagicDataBase.GetMagicData(magicType);
                    BaseActionFunctions.UseMagic(actor, target, magicType);
                    break;
            }

            return true;
        }

        public void ExecuteAction(BaseCharacter target)
        {
            switch (actionType)
            {
                case BaseActionType.NormalAttack:
                    BaseActionFunctions.NormalAttack(actor, target);
                    break;
                case BaseActionType.UseItem:
                    var item = ItemDataBase.GetItemData(itemType);
                    BaseActionFunctions.UseItem(actor, target, itemType);
                    break;
            }
        }
    }
}
