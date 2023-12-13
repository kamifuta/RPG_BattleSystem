using InGame.Characters;
using InGame.Skills;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Log;

namespace InGame.Items
{
    public class MagicPotion : ItemData
    {
        public override string itemName => "–‚–@‚Ì¹…";
        public override string itemExplane => "MP‚ð­‚µ‰ñ•œ‚³‚¹‚é";
        public override ItemType itemType => ItemType.MagicPotion;
        public override TargetType targetType => TargetType.Friends;
        //public override bool IsTargetableDeadCharacter => false;

        private const int healingValue = 90;

        public override void ExecuteEffect(BaseCharacter target)
        {
            if (target.characterHealth.IsDead)
            {
                //LogWriter.WriteLog("‚µ‚©‚µ‰½‚à‹N‚±‚ç‚È‚©‚Á‚½");
                return;
            }

            var healing = new Healing(0, healingValue);
            target.Heal(healing);
        }
    }
}

