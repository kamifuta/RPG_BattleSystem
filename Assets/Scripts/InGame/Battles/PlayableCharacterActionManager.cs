using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Characters.PlayableCharacters;
using System;
using InGame.Characters;
using System.Linq;

namespace InGame.Buttles
{
    public class PlayableCharacterActionManager
    {
        private Dictionary<PlayableCharacter, ActionInfo> playableCharacterActionDic = new Dictionary<PlayableCharacter, ActionInfo>();

        public void SetPlayableCharacterAction(PlayableCharacter playableCharacter, ActionInfo action)
        {
            playableCharacterActionDic.Add(playableCharacter, action);
        }

        public void ClearDic()
        {
            playableCharacterActionDic.Clear();
        }

        public ActionInfo GetCharacterAction(PlayableCharacter playableCharacter)
            => playableCharacterActionDic[playableCharacter];

        public IEnumerable<ActionInfo> GetHighPriorityAction()
            => playableCharacterActionDic.Where(x => x.Value.priority > 0).Select(x => x.Value);
    }
}

