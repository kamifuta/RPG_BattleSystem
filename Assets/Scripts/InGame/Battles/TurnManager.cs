using InGame.Characters.PlayableCharacters;
using Log;
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
            LogWriter.WriteLog($"ターン{ turnCount}開始");
        }

        public void NextTurn()
        {
            LogWriter.WriteLog($"ターン{ turnCount}終了\n");
            turnCount++;
            LogWriter.WriteLog($"ターン{ turnCount}開始");
        }
    }
}

