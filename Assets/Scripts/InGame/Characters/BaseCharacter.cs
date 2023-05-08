using InGame.Damages;
using InGame.Healings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public class BaseCharacter
    {
        public CharacterStatus characterStatus { get; }
        public bool IsDead { get; private set; }

        private readonly CharacterHealth characterHealth;

        public BaseCharacter(CharacterStatus characterStatus)
        {
            this.characterStatus = characterStatus;

            characterHealth = new CharacterHealth(characterStatus.MaxHP);
        }

        public void Attack(BaseCharacter target)
        {
            Debug.Log("Attack");
            target.ApplyDamage(new Damage(characterStatus.AttackValue, DamageType.HP));
        }

        public void ApplyDamage(Damage damage)
        {
            var damageValue = damage.AttackValue - characterStatus.DefecnceValue;
            characterHealth.ApplyDamage(damageValue);
            IsDead = characterHealth.IsDead;
        }

        public void Heal(Healing healing)
        {

        }
    }
}

