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
        private readonly BaseCharacter attacker;
        private readonly DamageType damageType;

        public Damage(BaseCharacter attacker, DamageType damageType)
        {
            this.attacker = attacker;
            this.damageType = damageType;
        }

        public BaseCharacter Attacker => attacker;
        public int AttackValue => attacker.characterStatus.AttackValue;
        public DamageType DamageType => damageType;
    }
}

