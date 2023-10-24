using InGame.Skills;
using InGame.Damages;
using InGame.Healings;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Magics;
using System;

namespace InGame.Characters
{
    public class BaseCharacter : IDisposable
    {
        public string characterName { get; private set; }
        public CharacterStatus characterStatus { get; }

        public readonly CharacterHealth characterHealth;
        public readonly CharacterMagic characterMagic;
        public List<SkillType> rememberSkills { get; protected set; } = new List<SkillType>();
        public List<MagicType> rememberMagics { get; protected set; } = new List<MagicType>();

        public float HPRate => (float)characterHealth.currentHP / characterStatus.MaxHP;
        public float MPRate => (float)characterMagic.currentMP / characterStatus.MaxMP;

        public BaseCharacter(CharacterStatus characterStatus)
        {
            this.characterStatus = characterStatus;

            characterHealth = new CharacterHealth(characterStatus.MaxHP);
            characterMagic = new CharacterMagic(characterStatus.MaxMP);
        }

        public void SetCharacterName(string characterName)
        {
            this.characterName = characterName;
        }

        public void AddSkill(SkillType skillType)
        {
            rememberSkills.Add(skillType);
        }

        public void AddMagic(MagicType magicType)
        {
            rememberMagics.Add(magicType);
        }

        public virtual void ApplyDamage(Damage damage)
        {
            var damageValue = DamageCalculator.CalcDamage(damage, this);
            characterHealth.ApplyDamage(damageValue);
            //LogWriter.WriteLog($"{characterName}に{damageValue.ToString()}のダメージ");

            //if(characterHealth.IsDead)
                //LogWriter.WriteLog($"{characterName}は倒れた");
        }

        public void Heal(Healing healing)
        {
            if (healing.HealHPValue > 0)
            {
                var damagedValue = characterStatus.MaxHP - characterHealth.currentHP;
                var logValue = damagedValue > healing.HealHPValue ? healing.HealHPValue : damagedValue;
                //LogWriter.WriteLog($"{characterName}のHPが{logValue.ToString()}回復した");

            }
            characterHealth.Heal(healing.HealHPValue);

            if (healing.HealMPValue > 0)
            {
                var damagedValue = characterStatus.MaxMP - characterMagic.currentMP;
                var logValue = damagedValue > healing.HealMPValue ? healing.HealMPValue : damagedValue;
                //LogWriter.WriteLog($"{characterName}のMPが{logValue.ToString()}回復した");

            }
            characterMagic.HealMP(healing.HealMPValue);
        }

        public void Revaival()
        {
            //LogWriter.WriteLog($"{characterName}は復活した");
            characterHealth.Heal(characterStatus.MaxHP / 3);
        }

        public void FullHeal()
        {
            //デバッグ用
            characterHealth.Heal(10000);
            characterMagic.HealMP(10000);
        }

        public void Dispose()
        {
            characterHealth.Dispose();
        }
    }
}

