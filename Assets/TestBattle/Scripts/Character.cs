using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestButtle
{
    public class Character
    {
        private readonly string name;

        protected readonly int MaxHP;
        protected readonly int MaxMP;
        protected readonly int AttackValue;
        protected readonly int DefenceValue;

        public int currentHP { get; private set; }
        public int currentMP { get; private set; }

        public bool IsDead => currentHP <= 0;
        public bool HadDoneAction { get; private set; }

        public Character(string name, int maxHP, int maxMP, int attackValue, int defenceValue)
        {
            this.name = name;
            this.MaxHP = maxHP;
            this.MaxMP = maxMP;
            this.AttackValue = attackValue;
            this.DefenceValue = defenceValue;

            currentHP = maxHP;
            currentMP = maxMP;
        }

        public virtual void Attack(Character target)
        {
            //Debug.Log($"{name}‚ÌUŒ‚");
            target.ApplyDamage(AttackValue);
            HadDoneAction = true;
        }

        public virtual void ApplyDamage(int AttackValue)
        {
            currentHP -= AttackValue;
            //Debug.Log($"{name}‚ÌŽc‚è‘Ì—Í{currentHP}");
        }

        public virtual void Heal()
        {
            //Debug.Log($"{name}‚Í‰ñ•œ‚µ‚½");
            currentHP = MaxHP;
            currentMP--;
            HadDoneAction = true;
        }

        public void ClearFlag()
        {
            HadDoneAction = false;
        }
    }
}

