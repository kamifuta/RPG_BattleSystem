using InGame.Damages;
using Log;
using System.Collections;
using System.Collections.Generic;
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
        public static void NormalAttack(BaseCharacter user, BaseCharacter target, int consumeMP)
        {
            LogWriter.WriteLog($"{user.characterName}‚ÌUŒ‚");

            target.ApplyDamage(new Damage(user, DamageType.HP));
        }

        public static void Defence(BaseCharacter user, BaseCharacter target, int consumeMP )
        {
            user.characterStatus.characterBuff.SetIsDefencing(true);
            LogWriter.WriteLog($"{user.characterName}‚Íg‚ğç‚Á‚Ä‚¢‚é");
        }
    }
}

