using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public enum StatusType
    {
        HP,
        MP,
        Attack,
        Magic,
        Defence,
        MagicDefence,
        Agility,
    }

    public class CharacterStatus
    {
        public readonly int baseMaxHP;
        public readonly int baseMaxMP;
        public readonly int baseAttackValue;
        public readonly int baseMagicValue;
        public readonly int baseDefenceValue;
        public readonly int baseMagicDefenceValue;
        public readonly int baseAgility;

        public CharacterBuff characterBuff { get; private set; } = new CharacterBuff();

        public int MaxHP => baseMaxHP;
        public int MaxMP => baseMaxMP;
        public int AttackValue => Mathf.Sign(characterBuff.AttackBuffLevel) >= 0 ? Mathf.CeilToInt(baseAttackValue * ((2 + characterBuff.AttackBuffLevel) / 2)) : Mathf.CeilToInt(baseAttackValue * (2 / (2 - characterBuff.AttackBuffLevel)));
        public int MagicValue => Mathf.Sign(characterBuff.MagicBuffLevel) >= 0 ? Mathf.CeilToInt(baseMagicValue * ((2 + characterBuff.MagicBuffLevel) / 2)) : Mathf.CeilToInt(baseMagicValue * (2 / (2 - characterBuff.MagicBuffLevel)));
        public int DefenceValue => Mathf.Sign(characterBuff.DefenceBuffLevel) >= 0 ? Mathf.CeilToInt(baseDefenceValue * ((2 + characterBuff.DefenceBuffLevel) / 2)) : Mathf.CeilToInt(baseDefenceValue * (2 / (2 - characterBuff.DefenceBuffLevel)));
        public int MagicDefenceValue => Mathf.Sign(characterBuff.MagicDefenceBuffLevel) >= 0 ? Mathf.CeilToInt(baseMagicDefenceValue * ((2 + characterBuff.MagicDefenceBuffLevel) / 2)) : Mathf.CeilToInt(baseMagicDefenceValue * (2 / (2 - characterBuff.MagicDefenceBuffLevel)));
        public int Agility => Mathf.Sign(characterBuff.AgilityBuffLevel) >= 0 ? Mathf.CeilToInt(baseAgility * ((2 + characterBuff.AgilityBuffLevel) / 2)) : Mathf.CeilToInt(baseAgility * (2 / (2 - characterBuff.AgilityBuffLevel)));

        public CharacterStatus(CharacterStatusData statusData)
        {
            baseMaxHP = statusData.MaxHP;
            baseMaxMP = statusData.MaxMP;
            baseAttackValue = statusData.AttackValue;
            baseMagicValue = statusData.MagicValue;
            baseDefenceValue = statusData.DefenceValue;
            baseMagicDefenceValue = statusData.MagicDefenceValue;
            baseAgility = statusData.Agility;
        }

        public CharacterStatus(int maxHP, int maxMP, int attack, int magic, int defence, int magicDefence, int agility)
        {
            baseMaxHP = maxHP;
            baseMaxMP = maxMP;
            baseAttackValue = attack;
            baseMagicValue = magic;
            baseDefenceValue = defence;
            baseMagicDefenceValue = magicDefence;
            baseAgility = agility;
        }
    }
}

