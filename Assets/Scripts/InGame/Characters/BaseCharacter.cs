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
        public string characterName { get; private set; }
        public CharacterStatus characterStatus { get; }

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

        public void Attack(BaseCharacter target)
        {
            LogWriter.WriteLog($"{characterName}の攻撃");

            target.ApplyDamage(new Damage(characterStatus.AttackValue, DamageType.HP));
        }

        public void ApplyDamage(Damage damage)
        {
            var baseDamage = (damage.AttackValue / 2) - (characterStatus.DefecnceValue / 4);
            var damageValue = Mathf.CeilToInt(baseDamage + Random.Range(-0.16f, 0.16f) * baseDamage);
            characterHealth.ApplyDamage(damageValue);
            LogWriter.WriteLog($"{characterName}に{damageValue}のダメージ");

            if(characterHealth.IsDead)
                LogWriter.WriteLog($"{characterName}は倒れた");
        }

        public void Defence(BaseCharacter target)
        {
            characterStatus.characterBuff.SetIsDefencing(true);
            LogWriter.WriteLog($"{characterName}は身を守っている");
        }

        public void Heal(Healing healing)
        {

        }
    }
}

