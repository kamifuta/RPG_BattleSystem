using InGame.Characters.PlayableCharacters;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Buttles
{
    public class TurnManager
    {
        public int turnCount { get; private set; }

        public void StartTurn()
        {
            turnCount = 1;
            Debug.Log($"ターン{ turnCount}開始");
        }

        public void NextTurn()
        {
            turnCount++;
            Debug.Log($"ターン{ turnCount}開始");
        }
    }
}

