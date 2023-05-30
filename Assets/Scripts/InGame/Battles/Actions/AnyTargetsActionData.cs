using InGame.Characters;
using InGame.Items;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public class AnyTargetsActionData : ActionData
    {
        private readonly IEnumerable<BaseCharacter> targets;

        public AnyTargetsActionData(BaseActionType actionType, BaseCharacter actor, IEnumerable<BaseCharacter> targets, ItemType itemType):
            base(actionType, actor)
        {
            this.targets = targets;
        }

        public AnyTargetsActionData(BaseActionType actionType, BaseCharacter actor, IEnumerable<BaseCharacter> targets, SkillType skillType) :
            base(actionType, actor)
        {
            this.targets = targets;
        }

        public AnyTargetsActionData(BaseActionType actionType, BaseCharacter actor, IEnumerable<BaseCharacter> targets) :
            base(actionType, actor)
        {
            this.targets = targets;
        }
    }
}

