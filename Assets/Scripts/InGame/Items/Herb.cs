using InGame.Characters;
using InGame.Skills;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InGame.Items
{
    public class Herb : ItemData
    {
        public override string itemName => "��";
        public override string itemExplane => "�̗͂������񕜂���";
        public override ItemType itemType => ItemType.Herb;
        public override TargetType targetType => TargetType.Friends;
        public override bool IsTargetableDeadCharacter => false;

        private const int healingValue = 60;

        public override void ExecuteEffect(BaseCharacter target)
        {
            var healing = new Healing(healingValue, 0);
            target.Heal(healing);
        }
    }
}

