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
        Agility,
    }

    public class CharacterStatus
    {
        private readonly int baseMaxHP;
        private readonly int baseMaxMP;
        private readonly int baseAttackValue;
        private readonly int baseMagicValue;
        private readonly int baseDefenceValue;
        private readonly int baseMagicDefenceValue;
        private readonly int baseAgility;

        public CharacterBuff characterBuff { get; private set; } = new CharacterBuff();

        public int MaxHP => baseMaxHP;
        public int MaxMP => baseMaxMP;
        public int AttackValue => baseAttackValue;
        public int MagicValue => baseMagicValue;
        public int DefecnceValue => baseDefenceValue * (Convert.ToInt32(characterBuff.IsDefencing) * 2);
        public int MagicDefecnceValue => baseMagicDefenceValue * (Convert.ToInt32(characterBuff.IsDefencing) * 2);
        public int Agility => baseAgility;

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
    }
}

