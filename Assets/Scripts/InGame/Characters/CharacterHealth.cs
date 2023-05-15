using UnityEngine;

namespace InGame.Characters
{
    public class CharacterHealth
    {
        public int currentHP { get; private set; }

        public bool IsDead => currentHP <= 0;

        private readonly int MaxHP;

        public CharacterHealth(int maxHP)
        {
            MaxHP = maxHP;
            currentHP = maxHP;
        }

        public void ApplyDamage(int value)
        {
            currentHP -= value;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
        }

        public void Heal(int value)
        {
            currentHP += value;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
        }
    }
}

