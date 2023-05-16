using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Characters.PlayableCharacters;
using System;
using InGame.Characters;
using System.Linq;
using InGame.Characters.Skills;
using InGame.Buttles.Actions;

namespace InGame.Buttles
{
    public class PlayableCharacterActionManager
    {
        private Dictionary<PlayableCharacter, ActionData> playableCharacterActionDic = new Dictionary<PlayableCharacter, ActionData>();

        public void SetPlayableCharacterAction(PlayableCharacter playableCharacter, ActionData action)
        {
            playableCharacterActionDic.Add(playableCharacter, action);
        }

        public void ClearDic()
        {
            playableCharacterActionDic.Clear();
        }

        public KeyValuePair<PlayableCharacter, ActionData> GetCharacterActionPair(PlayableCharacter playableCharacter)
            => playableCharacterActionDic.Single(x => x.Key == playableCharacter);

        public ActionData GetCharacterAction(PlayableCharacter playableCharacter)
            => playableCharacterActionDic.Single(x => x.Key == playableCharacter).Value;

        public IEnumerable<KeyValuePair<PlayableCharacter, ActionData>> GetHighPriorityAction()
            => playableCharacterActionDic.Where(x => x.Value.skillData.priority > 0);
    }
}

