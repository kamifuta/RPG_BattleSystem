using InGame.Characters;
using InGame.Characters.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Items
{
    public abstract class Item
    {
        public abstract string ItemName { get; }
        public abstract string ItemExplane { get; }
        public abstract ItemType itemType { get; }
        public abstract TargetType targetType { get; }
        public abstract bool IsTargetableDeadCharacter { get; }

        public abstract void UseItem(BaseCharacter target);
        public abstract void UseItem(IEnumerable<BaseCharacter> targets);
    }
}

