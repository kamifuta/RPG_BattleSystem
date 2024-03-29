using InGame.Characters;
using InGame.Skills;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Log;

namespace InGame.Items
{
    public class Herb : ItemData
    {
        public override string itemName => "薬草";
        public override string itemExplane => "体力を少し回復する";
        public override ItemType itemType => ItemType.Herb;
        public override TargetType targetType => TargetType.Friends;
        //public override bool IsTargetableDeadCharacter => false;

        private const int healingValue = 60;

        public override void ExecuteEffect(BaseCharacter target)
        {
            if (target.characterHealth.IsDead)
            {
                //LogWriter.WriteLog("しかし何も起こらなかった");
                return;
            }

            var healing = new Healing(healingValue, 0);
            target.Heal(healing);
        }
    }
}

