using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Characters.PlayableCharacters;
using System;
using InGame.Characters;

namespace InGame.Buttles
{
    public class PlayableCharacterActionManager
    {
        private Dictionary<PlayableCharacter, Action<BaseCharacter>> playableCharacterActionDic;

        public void SetPlayableCharacterAction(PlayableCharacter playableCharacter, Action<BaseCharacter> action)
        {
            playableCharacterActionDic.Add(playableCharacter, action);
        }
    }
}

