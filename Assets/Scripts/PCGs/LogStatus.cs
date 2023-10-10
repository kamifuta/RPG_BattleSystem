using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCGs
{
    [Serializable]
    public class LogStatus
    {
        public int MaxHP;
        public int MaxMP;
        public int AttackValue;
        public int MagicValue;
        public int DefenceValue;
        public int MagicDefenceValue;
        public int Agility;

        public LogStatus(int maxHP, int maxMP, int attack, int magic, int defence, int magicDefence, int agility)
        {
            MaxHP = maxHP;
            MaxMP = maxMP;
            AttackValue = attack;
            MagicValue = magic;
            DefenceValue = defence;
            MagicDefenceValue = magicDefence;
            Agility = agility;
        }
    }
}

