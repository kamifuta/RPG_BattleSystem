using InGame.Characters;
using InGame.Characters.Skills;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InGame.Items
{
    public class Herb : Item
    {
        public override string ItemName => "–ò‘";
        public override string ItemExplane => "‘Ì—Í‚ð­‚µ‰ñ•œ‚·‚é";
        public override ItemType itemType => ItemType.Herb;
        public override TargetType targetType => TargetType.Friends;
        public override bool IsTargetableDeadCharacter => false;

        private const int healingValue = 60;

        public override void UseItem(BaseCharacter target)
        {
            var healing = new Healing(healingValue, 0);
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

