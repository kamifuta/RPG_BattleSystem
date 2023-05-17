using InGame.Characters;
using InGame.Characters.Skills;
using InGame.Damages;
using InGame.Items;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public static class BaseActionFunctions
    {
        public static void NormalAttack(BaseCharacter actor, BaseCharacter target)
        {
            LogWriter.WriteLog($"{actor.characterName}ÇÃçUåÇ");

            target.ApplyDamage(new Damage(actor, actor.characterStatus.AttackValue, DamageType.HP));
        }

        public static void Defence(BaseCharacter actor)
        {
            LogWriter.WriteLog($"{actor.characterName}ÇÕêgÇéÁÇ¡ÇƒÇ¢ÇÈ");

            actor.characterStatus.characterBuff.SetIsDefencing(true);
        }

        public static void UseItem(BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            var item = ItemDataBase.GetItemData(itemType);

            LogWriter.WriteLog($"{actor.characterName}ÇÕ{item.itemName}ÇégópÇµÇΩ");
            
            item.ExecuteEffect(target);
        }

        public static void UseSkill(BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);

            LogWriter.WriteLog($"{actor.characterName}ÇÃ{skill.skillName}");

            skill.ExecuteSkill(actor, target);
        }
    }
}

