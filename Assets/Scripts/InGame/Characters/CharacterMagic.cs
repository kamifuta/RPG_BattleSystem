using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterMagic
    {
        public int currentMP { get; private set; }

        private readonly int MaxMP;

        private readonly ISubject<int> healValueSubject = new Subject<int>();
        public IObservable<int> HealValueObservable => healValueSubject;

        public CharacterMagic(int maxMP)
        {
            MaxMP = maxMP;
            currentMP = maxMP;
        }

        public void DecreaseMP(int value)
        {
            currentMP -= value;
            currentMP = Mathf.Clamp(currentMP, 0, MaxMP);
        }

        public void HealMP(int value)
        {
            currentMP += value;
            currentMP = Mathf.Clamp(currentMP, 0, MaxMP);

            healValueSubject.OnNext(value);
        }
    }
}

