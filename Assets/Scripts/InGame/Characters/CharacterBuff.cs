using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterBuff
    {
        private sealed class BuffTurn
        {
            public readonly StatusType statusType;
            public int remainingTurn;

            public BuffTurn(StatusType statusType, int remainingTurn)
            {
                this.statusType = statusType;
                this.remainingTurn = remainingTurn;
            }
        }

        private const int MaxBuffLevel = 2;
        private const int MinBuffLevel = -2;

        public bool IsDefencing { get; private set; }

        public int AttackBuffLevel { get; private set; } = 0;
        public int MagicBuffLevel { get; private set; } = 0;
        public int DefenceBuffLevel { get; private set; } = 0;
        public int MagicDefenceBuffLevel { get; private set; } = 0;
        public int AgilityBuffLevel { get; private set; } = 0;

        private readonly List<BuffTurn> buffTurnList = new List<BuffTurn>(8);

        public void TryDeleteBuff()
        {
            SetIsDefencing(false);

            foreach(var buff in buffTurnList)
            {
                buff.remainingTurn--;

                if (buff.remainingTurn <= 0)
                {
                    DeleteBuff(buff.statusType);
                }
            }
        }

        private void DeleteBuff(StatusType statusType)
        {
            switch (statusType)
            {
                case StatusType.Attack:
                    AttackBuffLevel = 0;
                    break;
                case StatusType.Magic:
                    MagicBuffLevel = 0;
                    break;
                case StatusType.Defence:
                    DefenceBuffLevel = 0;
                    break;
                case StatusType.MagicDefence:
                    MagicDefenceBuffLevel = 0;
                    break;
                case StatusType.Agility:
                    AgilityBuffLevel = 0;
                    break;
            }
        }

        public void SetIsDefencing(bool value)
        {
            if (IsDefencing == value)
                return;

            IsDefencing = value;

            var level = value ? 1 : -1;
            DefenceBuffLevel += level;
            MagicDefenceBuffLevel += level;
        }

        public void RaiseAttackBuffLevel(int level)
        {
            AttackBuffLevel += level;

            AttackBuffLevel = Mathf.Clamp(AttackBuffLevel, MinBuffLevel, MaxBuffLevel);
            SetBuffTurn(StatusType.Attack);
        }

        public void RaiseMagicBuffLevel(int level)
        {
            MagicBuffLevel += level;

            MagicBuffLevel = Mathf.Clamp(MagicBuffLevel, MinBuffLevel, MaxBuffLevel);
            SetBuffTurn(StatusType.Magic);
        }

        public void RaiseDefenceBuffLevel(int level)
        {
            DefenceBuffLevel += level;

            DefenceBuffLevel = Mathf.Clamp(DefenceBuffLevel, MinBuffLevel, MaxBuffLevel);
            SetBuffTurn(StatusType.Defence);
        }

        public void RaiseMagicDefenceBuffLevel(int level)
        {
            MagicDefenceBuffLevel += level;

            MagicDefenceBuffLevel = Mathf.Clamp(MagicDefenceBuffLevel, MinBuffLevel, MaxBuffLevel);
            SetBuffTurn(StatusType.MagicDefence);
        }

        public void RaiseAgilityBuffLevel(int level)
        {
            AgilityBuffLevel += level;

            AgilityBuffLevel = Mathf.Clamp(AgilityBuffLevel, MinBuffLevel, MaxBuffLevel);
            SetBuffTurn(StatusType.Agility);
        }

        private void SetBuffTurn(StatusType statusType)
        {
            var buff = buffTurnList.FirstOrDefault(x => x.statusType == statusType);

            if (buff == null)
            {
                buffTurnList.Add(new BuffTurn(statusType, 3));
            }
            else
            {
                buff.remainingTurn = 3;
            }
        }
    }
}