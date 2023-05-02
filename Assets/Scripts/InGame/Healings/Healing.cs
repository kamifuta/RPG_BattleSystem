using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Healings
{
    public class Healing
    {
        private readonly int healHPValue;
        private readonly int healMPValue;

        public Healing(int healHPValue, int healMPValue)
        {
            this.healHPValue = healHPValue;
            this.healMPValue = healMPValue;
        }

        public int HealHPValue => healHPValue;
        public int HealMPValue => healMPValue;
    }
}

