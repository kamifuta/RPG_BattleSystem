using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public class AnyTargetsActionData : ActionData
    {
        private readonly IEnumerable<BaseCharacter> targets;

        public AnyTargetsActionData(BaseActionType actionType, BaseCharacter actor, IEnumerable<BaseCharacter> targets):
            base(actionType, actor)
        {
            this.targets = targets;
        }
    }
}

