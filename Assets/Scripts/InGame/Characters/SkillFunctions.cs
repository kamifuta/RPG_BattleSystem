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
            LogWriter.WriteLog($"{actionArgument.user.characterName}�̍U��");

            var target = actionArgument.targets.Single();
            target.ApplyDamage(new Damage(actionArgument.user, DamageType.HP));
        }

        public static void Defence(ActionArgument actionArgument, int consumeMP)
        {
            actionArgument.user.characterStatus.characterBuff.SetIsDefencing(true);
            LogWriter.WriteLog($"{actionArgument.user.characterName}�͐g������Ă���");
        }

        public static void UseItem(ActionArgument actionArgument, int consumeMP)
        {
            if(actionArgument is UseItemActionArgument)
            {
                Debug.LogError("�����̌^���s���ł�");
            }

            LogWriter.WriteLog($"{actionArgument.user.characterName}�̓A�C�e�����g�p����");
            var arg = actionArgument as UseItemActionArgument;
            var Item = ItemDataBase.GetItemData(arg.useItemType);
            Item.UseItem(arg.targets);
        }
    }
}

