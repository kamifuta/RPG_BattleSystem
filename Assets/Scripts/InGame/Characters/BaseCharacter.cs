using InGame.Damages;
using InGame.Healings;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public class BaseCharacter
    {
        public string characterName;
        public CharacterStatus characterStatus { get; }
        //public bool IsDead { get; private set; }

        public readonly CharacterHealth characterHealth;

        public BaseCharacter(CharacterStatus characterStatus)
        {
            this.characterStatus = characterStatus;

            characterHealth = new CharacterHealth(characterStatus.MaxHP);
        }

        public void SetCharacterName(string characterName)
        {
            this.characterName = characterName;
        }

        public void SetCharacterNameIdentifier(int identifier)
        {
            characterName = $"{characterName}_{identifier}";
        }

        public void Attack(BaseCharacter target)
        {
            LogWriter.WriteLog($"{characterName}ÇÃçUåÇ");

            target.ApplyDamage(new Damage(characterStatus.AttackValue, DamageType.HP));
        }

        public void ApplyDamage(Damage damage)
        {
            var baseDamage = (damage.AttackValue / 2) - (characterStatus.DefecnceValue / 4);
            var damageValue = Mathf.CeilToInt(baseDamage + Random.Range(-0.16f, 0.16f) * baseDamage);
            characterHealth.ApplyDamage(damageValue);
            LogWriter.WriteLog($"{characterName}Ç…{damageValue}ÇÃÉ_ÉÅÅ[ÉW");

            if(characterHealth.IsDead)
                LogWriter.WriteLog($"{characterName}ÇÕì|ÇÍÇΩ");
        }

        public void Heal(Healing healing)
        {

        }
    }
}

