using InGame.Skills;
using InGame.Damages;
using InGame.Healings;
using Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Magics;

namespace InGame.Characters
{
    public class BaseCharacter
    {
        public string characterName { get; private set; }
        public CharacterStatus characterStatus { get; }

        public readonly CharacterHealth characterHealth;
        public readonly CharacterMagic characterMagic;
        public List<SkillType> rememberSkills { get; private set; } = new List<SkillType>();
        public List<MagicType> rememberMagics { get; private set; } = new List<MagicType>();

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
            var damageValue = CalcDamage(damage);
            characterHealth.ApplyDamage(damageValue);
            LogWriter.WriteLog($"{characterName}に{damageValue}のダメージ");

            if(characterHealth.IsDead)
                LogWriter.WriteLog($"{characterName}は倒れた");
        }

        protected int CalcDamage(Damage damage)
        {
            int baseDamageValue = 0;
            switch (damage.attackType)
            {
                case AttackType.Physics:
                    baseDamageValue = (damage.attackValue / 2) - (characterStatus.DefecnceValue / 4);
                    break;
                case AttackType.Magic:
                    baseDamageValue = (damage.attackValue / 2) - (characterStatus.MagicDefecnceValue / 4);
                    break;
            }

            var damageValue = Mathf.CeilToInt(baseDamageValue + Random.Range(-0.16f, 0.16f) * baseDamageValue);
            if (damageValue < 0)
            {
                damageValue = 1;
            }

            return damageValue;
        }

        public void Heal(Healing healing)
        {
            if (healing.HealHPValue > 0)
            {
                var damagedValue = characterStatus.MaxHP - characterHealth.currentHP;
                var logValue = damagedValue > healing.HealHPValue ? healing.HealHPValue : damagedValue;
                LogWriter.WriteLog($"{characterName}のHPが{logValue}回復した");

            }
            characterHealth.Heal(healing.HealHPValue);

            if (healing.HealMPValue > 0)
            {
                var damagedValue = characterStatus.MaxMP - characterMagic.currentMP;
                var logValue = damagedValue > healing.HealMPValue ? healing.HealMPValue : damagedValue;
                LogWriter.WriteLog($"{characterName}のMPが{logValue}回復した");

            }
            characterMagic.HealMP(healing.HealMPValue);
        }

        public void Revaival()
        {
            LogWriter.WriteLog($"{characterName}は復活した");
            characterHealth.Heal(characterStatus.MaxHP / 3);
        }

        public void FullHeal()
        {
            //デバッグ用
            characterHealth.Heal(10000);
            characterMagic.HealMP(10000);
        }
    }
}

