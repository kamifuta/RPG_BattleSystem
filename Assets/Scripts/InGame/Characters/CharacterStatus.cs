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
        public int AttackValue => Mathf.CeilToInt(baseAttackValue * (1 + characterBuff.AttackBuffLevel * 0.5f));
        public int MagicValue => Mathf.CeilToInt(baseMagicValue * (1 + characterBuff.MagicBuffLevel * 0.5f));
        public int DefenceValue => Mathf.CeilToInt(baseDefenceValue * (1 + characterBuff.DefenceBuffLevel * 0.5f));
        public int MagicDefenceValue => Mathf.CeilToInt(baseMagicDefenceValue * (1 + characterBuff.MagicDefenceBuffLevel * 0.5f));
        public int Agility => Mathf.CeilToInt(baseAgility * (1 + characterBuff.AgilityBuffLevel * 0.5f));

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

