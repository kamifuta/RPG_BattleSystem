using InGame.Characters;
using InGame.Characters.Skills;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Items
{
    public class MagicPotion : Item
    {
        public override string ItemName => "–‚–@‚Ì¹…";
        public override string ItemExplane => "MP‚ð­‚µ‰ñ•œ‚³‚¹‚é";
        public override ItemType itemType => ItemType.MagicPotion;
        public override TargetType targetType => TargetType.Friends;
        public override bool IsTargetableDeadCharacter => false;

        private const int healingValue = 60;

        public override void UseItem(BaseCharacter target)
        {
            var healing = new Healing(0, healingValue);
            target.Heal(healing);
        }

        public override void UseItem(IEnumerable<BaseCharacter> targets)
        {
            if (targets.Count() != 1)
                return;

            UseItem(targets.Single());
        }
    }
}

