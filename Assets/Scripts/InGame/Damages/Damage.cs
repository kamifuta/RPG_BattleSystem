using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Damages
{
    public enum DamageTargetType
    {
        //None=0b_0000_0000,
        HP=0b_0000_0001,
        MP=0b_0000_0010,
    }

    public enum AttackType
    {
        Physics,
        Magic,
    }

    public enum DamageAttributeType
    {
        None,
        Fire,
        Ice
    }

    public class Damage
    {
        public BaseCharacter attacker { get; }
        public int attackValue { get; }
        public DamageTargetType damageType { get; }
        public AttackType attackType { get; }
        public DamageAttributeType attribute { get; }

        public Damage(BaseCharacter attacker, int attackValue, DamageTargetType damageType, AttackType attackType, DamageAttributeType attribute)
        {
            this.attacker = attacker;
            this.attackValue = attackValue;
            this.damageType = damageType;
            this.attackType = attackType;
            this.attribute = attribute;
        }
    }
}

