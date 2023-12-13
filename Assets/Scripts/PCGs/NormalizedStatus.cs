using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCGs
{
    public class NormalizedStatus
    {
        public NormalizedStatus(float HP, float MP, float Attack, float Magic, float Defence, float MagicDefence, float Agility)
        {
            this.HP = HP;
            this.MP = MP;
            this.Attack = Attack;
            this.Magic = Magic;
            this.Defence = Defence;
            this.MagicDefence = MagicDefence;
            this.Agility = Agility;
        }

        public readonly float HP;
        public readonly float MP;
        public readonly float Attack;
        public readonly float Magic;
        public readonly float Defence;
        public readonly float MagicDefence;
        public readonly float Agility;
    }
}

