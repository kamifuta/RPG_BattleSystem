using InGame.Characters;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Items
{
    public class Herb : Item
    {
        public override string ItemName => "��";
        public override string ItemExplane => "�̗͂������񕜂���";
        public override ItemType itemType => ItemType.Herb;

        private const int healingValue = 60;

        public override void UseItem(BaseCharacter target)
        {
            var healing = new Healing(healingValue, 0);
            target.Heal(healing);
        }
    }
}

