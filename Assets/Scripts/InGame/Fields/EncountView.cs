using InGame.Characters.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Fields
{
    public class EncountView : MonoBehaviour
    {
        [SerializeField] private Button button;

        private ISubject<EnemyType> encountSubject = new Subject<EnemyType>();
        public IObservable<EnemyType> EncountObservable => encountSubject;

        private void Start()
        {
            button.onClick.AddListener(() => encountSubject.OnNext(EnemyType.Slime));
        }
    }
}

