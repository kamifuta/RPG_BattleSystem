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
            target.ApplyDamage(new Damage(actor, actor.characterStatus.AttackValue, DamageTargetType.HP, AttackType.Physics, DamageAttributeType.None));
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, "通常攻撃");
#endif
        }

        public static void Defence(BaseCharacter actor)
        {
            actor.characterStatus.characterBuff.SetIsDefencing(true);
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, "防御");
#endif
        }

        public static void UseItem(BaseCharacter actor, BaseCharacter target, ItemType itemType)
        {
            var item = ItemDataBase.GetItemData(itemType);

            item.ExecuteEffect(target);
            (actor as PlayableCharacter).RemoveItem(itemType);
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, "アイテム使用");
#endif
        }

        public static void UseSkill(BaseCharacter actor, BaseCharacter target, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);

            skill.ExecuteSkill(actor, target);
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, skill.skillName);
#endif
        }

        public static void UseSkill(BaseCharacter actor, IEnumerable<BaseCharacter> targets, SkillType skillType)
        {
            var skill = SkillDataBase.GetSkillData(skillType);

            skill.ExecuteSkill(actor, targets);
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, skill.skillName);
#endif
        }

        public static void UseMagic(BaseCharacter actor, BaseCharacter target, MagicType magicType)
        {
            var magic = MagicDataBase.GetMagicData(magicType);

            magic.ExecuteMagic(actor, target);
#if Analyze
            PCGLog.WriteActionCSV(actor.characterName, magic.magicName);
#endif
        }
    }
}

