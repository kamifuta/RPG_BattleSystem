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
            LogWriter.WriteLog($"{actor.characterName}�̍U��");

            target.ApplyDamage(new Damage(actor, actor.characterStatus.AttackValue, DamageType.HP));
        }

        public static void Defence(BaseCharacter actor)
        {
            LogWriter.WriteLog($"{actor.characterName}�͐g������Ă���");

            actor.characterStatus.characterBuff.SetIsDefencing(true);
        }

        public static void UseItem(BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            var item = ItemDataBase.GetItemData(itemType);

            LogWriter.WriteLog($"{actor.characterName}��{item.itemName}���g�p����");
            
            item.ExecuteEffect(target);
        }

        public static void UseSkill(BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);

            LogWriter.WriteLog($"{actor.characterName}��{skill.skillName}");

            skill.ExecuteSkill(actor, target);
        }
    }
}

