using InGame.Characters;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Items
{
    public class MagicPotion : Item
    {
        public override string ItemName => "魔法の聖水";
        public override string ItemExplane => "MPを少し回復させる";
        public override ItemType itemType => ItemType.MagicPotion;

        private const int healingValue = 60;

        public override void UseItem(BaseCharacter target)
        {
            var healing = new Healing(0, healingValue);
            target.Heal(healing);
        }
    }
}

