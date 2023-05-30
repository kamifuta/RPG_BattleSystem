using System;
using UniRx;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterHealth
    {
        public int currentHP { get; private set; }

        public bool IsDead => currentHP <= 0;

        private readonly int MaxHP;

        private readonly ISubject<int> healValueSubject = new Subject<int>();
        public IObservable<int> HealValueObservable => healValueSubject;

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

            healValueSubject.OnNext(value);
        }
    }
}

