using InGame.Skills;
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
        public readonly CharacterMagic characterMagic;
        //public List<SkillType> rememberSkills { get; private set; } = new List<SkillType>() { SkillType.NormalAttack, SkillType.Defence };

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

        public virtual void ApplyDamage(Damage damage)
        {
            int baseDamageValue=0;
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
                damageValue = Random.Range(0, 2);
            }
            characterHealth.ApplyDamage(damageValue);
            LogWriter.WriteLog($"{characterName}��{damageValue}�̃_���[�W");

            if(characterHealth.IsDead)
                LogWriter.WriteLog($"{characterName}�͓|�ꂽ");
        }

        public void Heal(Healing healing)
        {
            if (healing.HealHPValue > 0)
            {
                var damagedValue = characterStatus.MaxHP - characterHealth.currentHP;
                var logValue = damagedValue > healing.HealHPValue ? healing.HealHPValue : damagedValue;
                LogWriter.WriteLog($"{characterName}��HP��{logValue}�񕜂���");

            }
            characterHealth.Heal(healing.HealHPValue);

            if (healing.HealMPValue > 0)
            {
                var damagedValue = characterStatus.MaxMP - characterMagic.currentMP;
                var logValue = damagedValue > healing.HealMPValue ? healing.HealMPValue : damagedValue;
                LogWriter.WriteLog($"{characterName}��MP��{logValue}�񕜂���");

            }
            characterMagic.HealMP(healing.HealMPValue);
        }

        public void Revaival()
        {
            LogWriter.WriteLog($"{characterName}�͕�������");
            characterHealth.Heal(characterStatus.MaxHP / 3);
        }
    }
}

