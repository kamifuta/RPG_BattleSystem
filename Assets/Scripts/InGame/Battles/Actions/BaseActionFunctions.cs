using InGame.Characters;
using InGame.Skills;
using InGame.Damages;
using InGame.Items;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Magics;
using InGame.Characters.PlayableCharacters;

namespace InGame.Buttles.Actions
{
    public static class BaseActionFunctions
    {
        public static void NormalAttack(BaseCharacter actor, BaseCharacter target)
        {
            //LogWriter.WriteLog($"{actor.characterName}の攻撃");
            //Debug.Log("<color=red>攻撃</color>");

            target.ApplyDamage(new Damage(actor, actor.characterStatus.AttackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None));
        }

        public static void Defence(BaseCharacter actor)
        {
            //LogWriter.WriteLog($"{actor.characterName}は身を守っている");
            //Debug.Log("<color=red>防御</color>");

            actor.characterStatus.characterBuff.SetIsDefencing(true);
        }

        public static void UseItem(BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            var item = ItemDataBase.GetItemData(itemType);
            //Debug.Log($"<color=red>{itemType}を使用</color>");

            //LogWriter.WriteLog($"{actor.characterName}は{item.itemName}を使用した");

            item.ExecuteEffect(target);
            (actor as PlayableCharacter).RemoveItem(itemType);
        }

        public static void UseSkill(BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);
            //Debug.Log($"<color=red>{skillType}を使用</color>");

            //LogWriter.WriteLog($"{actor.characterName}の{skill.skillName}");

            skill.ExecuteSkill(actor, target);
        }

        public static void UseMagic(BaseCharacter actor, BaseCharacter target, MagicType magicType)
        {
            var magic = MagicDataBase.GetMagicData(magicType);
            //Debug.Log($"<color=red>{magicType}を使用</color>");

            //LogWriter.WriteLog($"{actor.characterName}の{magic.magicName}");

            magic.ExecuteMagic(actor, target);
        }
    }
}

