using InGame.Buttles.Actions;
using InGame.Damages;
using InGame.Items;
using Log;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Characters.Skills
{
    public enum SkillType
    {
        NormalAttack,
        PowerAttack,
        FirstAttack,
        LastAttack,
        Defence,
        BigDefence,
        UseItem,
    }

    public static class SkillFunctions
    {
        public static void NormalAttack(ActionArgument actionArgument, int consumeMP)
        {
            LogWriter.WriteLog($"{actionArgument.user.characterName}の攻撃");

            var target = actionArgument.targets.Single();
            target.ApplyDamage(new Damage(actionArgument.user, DamageType.HP));
        }

        public static void Defence(ActionArgument actionArgument, int consumeMP)
        {
            actionArgument.user.characterStatus.characterBuff.SetIsDefencing(true);
            LogWriter.WriteLog($"{actionArgument.user.characterName}は身を守っている");
        }

        public static void UseItem(ActionArgument actionArgument, int consumeMP)
        {
            if(actionArgument is UseItemActionArgument)
            {
                Debug.LogError("引数の型が不正です");
            }

            LogWriter.WriteLog($"{actionArgument.user.characterName}はアイテムを使用した");
            var arg = actionArgument as UseItemActionArgument;
            var Item = ItemDataBase.GetItemData(arg.useItemType);
            Item.UseItem(arg.targets);
        }
    }
}

