using InGame.Buttles;
using InGame.Characters.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Fields
{
    public class FieldManager
    {
        public EmargingEnemiesList emargingEnemiesList { get; private set; }

        private ISubject<EnemyType> encountedEnemySubject = new Subject<EnemyType>();
        public IObservable<EnemyType> EncountedEnemyObservable => encountedEnemySubject;

        public void EncountEnemy(EnemyType encountedEnemyType)
        {
            encountedEnemySubject.OnNext(encountedEnemyType);
        }
    }
}

