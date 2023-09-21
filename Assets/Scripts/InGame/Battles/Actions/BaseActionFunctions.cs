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
            //LogWriter.WriteLog($"{actor.characterName}�̍U��");
            //Debug.Log("<color=red>�U��</color>");

            target.ApplyDamage(new Damage(actor, actor.characterStatus.AttackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None));
        }

        public static void Defence(BaseCharacter actor)
        {
            //LogWriter.WriteLog($"{actor.characterName}�͐g������Ă���");
            //Debug.Log("<color=red>�h��</color>");

            actor.characterStatus.characterBuff.SetIsDefencing(true);
        }

        public static void UseItem(BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            var item = ItemDataBase.GetItemData(itemType);
            //Debug.Log($"<color=red>{itemType}���g�p</color>");

            //LogWriter.WriteLog($"{actor.characterName}��{item.itemName}���g�p����");

            item.ExecuteEffect(target);
            (actor as PlayableCharacter).RemoveItem(itemType);
        }

        public static void UseSkill(BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);
            //Debug.Log($"<color=red>{skillType}���g�p</color>");

            //LogWriter.WriteLog($"{actor.characterName}��{skill.skillName}");

            skill.ExecuteSkill(actor, target);
        }

        public static void UseMagic(BaseCharacter actor, BaseCharacter target, MagicType magicType)
        {
            var magic = MagicDataBase.GetMagicData(magicType);
            //Debug.Log($"<color=red>{magicType}���g�p</color>");

            //LogWriter.WriteLog($"{actor.characterName}��{magic.magicName}");

            magic.ExecuteMagic(actor, target);
        }
    }
}

