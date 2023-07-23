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
            //LogWriter.WriteLog($"�^�[��{ turnCount.ToString()}�J�n");
        }

        public void NextTurn()
        {
            //LogWriter.WriteLog($"�^�[��{ turnCount.ToString()}�I��\n");
            turnCount++;
            //LogWriter.WriteLog($"�^�[��{ turnCount.ToString()}�J�n");
        }
    }
}

