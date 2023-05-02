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
        private readonly int attackValue;
        private readonly DamageType damageType;

        public Damage(int attackValue, DamageType damageType)
        {
            this.attackValue = attackValue;
            this.damageType = damageType;
        }

        public int AttackValue => attackValue;
        public DamageType DamageType => damageType;
    }
}

