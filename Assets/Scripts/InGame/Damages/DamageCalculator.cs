using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Damages
{
    public static class DamageCalculator
    {
        public static int CalcDamage(Damage damage, BaseCharacter target)
        {
            int baseDamageValue = 0;
            switch (damage.attackType)
            {
                case AttackType.Physics:
                    baseDamageValue = (damage.attackValue / 2) - (target.characterStatus.DefenceValue / 4);
                    break;
                case AttackType.Magic:
                    baseDamageValue = (damage.attackValue / 2) - (target.characterStatus.MagicDefenceValue / 4);
                    break;
            }

            var damageValue = Mathf.CeilToInt(baseDamageValue + Random.Range(-0.16f, 0.16f) * baseDamageValue);
            if (damageValue < 0)
            {
                damageValue = 1;
            }

            return damageValue;
        }
    }

}
