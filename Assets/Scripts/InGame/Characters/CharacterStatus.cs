using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterStatus
    {
        private readonly int baseMaxHP;
        private readonly int baseMaxMP;
        private readonly int baseAttackValue;
        private readonly int baseDefenceValue;
        private readonly int baseAgility;

        public int MaxHP => baseMaxHP;
        public int MaxMP => baseMaxMP;
        public int AttackValue => baseAttackValue;
        public int DefecnceValue => baseDefenceValue;
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

