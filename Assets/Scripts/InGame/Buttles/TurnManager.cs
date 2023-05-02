using InGame.Characters.PlayableCharacters;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Buttles
{
    public enum TurnType
    {
        ActionSelect,
        Buttle,
    }

    public class TurnManager
    {
        private ReactiveProperty<TurnType> currentTurn;
        public IObservable<TurnType> CurrentTurn => currentTurn;

        private int currentTurnCount = 0;

        public void StartPlayerActionSelectTurn()
        {

        }

        public void EndPlayerActionSelectTurn()
        {

        }

        public void StartButtleTurn()
        {

        }

        private void EndButtleTurn()
        {

        }
    }
}

