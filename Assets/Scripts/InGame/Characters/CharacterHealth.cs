using System;
using UniRx;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterHealth : IDisposable
    {
        public int currentHP { get; private set; }

        public bool IsDead => currentHP <= 0;

        private readonly int MaxHP;

        private readonly Subject<int> healValueSubject = new Subject<int>();
        public IObservable<int> HealValueObservable => healValueSubject;

        private readonly Subject<int> damagedValueSubject = new Subject<int>();
        public IObservable<int> DamagedValueObservable => damagedValueSubject;

        public CharacterHealth(int maxHP)
        {
            MaxHP = maxHP;
            currentHP = maxHP;
        }

        public void ApplyDamage(int value)
        {
            currentHP -= value;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);

            damagedValueSubject.OnNext(value);
        }

        public void Heal(int value)
        {
            currentHP += value;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);

            healValueSubject.OnNext(value);
        }

        public void Dispose()
        {
            healValueSubject.Dispose();
            damagedValueSubject.Dispose();
        }
    }
}

