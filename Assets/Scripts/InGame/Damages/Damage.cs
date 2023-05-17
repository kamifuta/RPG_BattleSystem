using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Damages
{
    public enum DamageType
    {
        //None=0b_0000_0000,
        HP=0b_0000_0001,
        MP=0b_0000_0010,
    }

    public class Damage
    {
        public BaseCharacter attacker { get; }
        public int attackValue { get; }
        public DamageType damageType { get; }

        public Damage(BaseCharacter attacker, int attackValue, DamageType damageType)
        {
            this.attacker = attacker;
            this.attackValue = attackValue;
            this.damageType = damageType;
        }
    }
}

