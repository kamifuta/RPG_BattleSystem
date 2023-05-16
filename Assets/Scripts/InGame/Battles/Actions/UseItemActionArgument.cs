using InGame.Characters;
using InGame.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public class UseItemActionArgument : ActionArgument
    {
        public ItemType useItemType { get; private set; }

        public UseItemActionArgument(BaseCharacter user, IEnumerable<BaseCharacter> targets, ItemType useItemType) :
            base(user, targets)
        {
            this.useItemType = useItemType;
        }

        public UseItemActionArgument(BaseCharacter user, BaseCharacter target, ItemType useItemType):
            base(user, target)
        {
            this.useItemType = useItemType;
        }
    }
}

