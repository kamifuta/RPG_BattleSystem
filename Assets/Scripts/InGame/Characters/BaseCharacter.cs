using InGame.Characters.Skills;
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
        public bool HadDoneAction { get; private set; } = false;
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
            var baseDamage = (damage.attackValue / 2) - (characterStatus.DefecnceValue / 4);
            var damageValue = Mathf.CeilToInt(baseDamage + Random.Range(-0.16f, 0.16f) * baseDamage);
            if (damageValue < 0)
            {
                damageValue = Random.Range(0, 2);
            }
            characterHealth.ApplyDamage(damageValue);
            LogWriter.WriteLog($"{characterName}‚É{damageValue}‚Ìƒ_ƒ[ƒW");

            if(characterHealth.IsDead)
                LogWriter.WriteLog($"{characterName}‚Í“|‚ê‚½");
        }

        public void Heal(Healing healing)
        {
            if(healing.HealHPValue>0)
                LogWriter.WriteLog($"{characterName}‚ÌHP‚ª{healing.HealHPValue}‰ñ•œ‚µ‚½");
            characterHealth.Heal(healing.HealHPValue);

            if(healing.HealMPValue>0)
                LogWriter.WriteLog($"{characterName}‚ÌMP‚ª{healing.HealMPValue}‰ñ•œ‚µ‚½");
            characterMagic.HealMP(healing.HealMPValue);
        }

        public void SetHadDoneAction(bool value)
        {
            HadDoneAction = value;
        }

        public void ResetFlag()
        {
            HadDoneAction = false;
        }
    }
}

