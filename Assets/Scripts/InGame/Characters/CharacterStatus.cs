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
        Defence,
        Agility,
    }

    public class CharacterStatus
    {
        private readonly int baseMaxHP;
        private readonly int baseMaxMP;
        private readonly int baseAttackValue;
        private readonly int baseDefenceValue;
        private readonly int baseAgility;

        public CharacterBuff characterBuff { get; private set; } = new CharacterBuff();

        public int MaxHP => baseMaxHP;
        public int MaxMP => baseMaxMP;
        public int AttackValue => baseAttackValue;
        public int DefecnceValue => baseDefenceValue * (Convert.ToInt32(characterBuff.IsDefencing) * 2);
        public int Agility => baseAgility;

        public CharacterStatus(CharacterStatusData statusData)
        {
            baseMaxHP = statusData.MaxHP;
            baseMaxMP = statusData.MaxMP;
            baseAttackValue = statusData.AttackValue;
            baseDefenceValue = statusData.DefenceValue;
            baseAgility = statusData.Agility;
        }
    }
}

