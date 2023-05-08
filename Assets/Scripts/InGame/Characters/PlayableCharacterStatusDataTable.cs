using InGame.Characters.Enemies;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Characters.PlayableCharacters;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/PlayableCharacterStatusDataTable", fileName = "PlayableCharacterStatusDataTable")]
    public class PlayableCharacterStatusDataTable : ScriptableObject
    {
        [Serializable]
        public class PlayableCharacterStatusData
        {
            [SerializeField] private PlayableCharacterType playableCharacterType;
            [SerializeField] private CharacterStatusData characterStatusData;

            public PlayableCharacterType PlayableCharacterType => playableCharacterType;
            public CharacterStatusData CharacterStatusData => characterStatusData;
        }

        [SerializeField] private List<PlayableCharacterStatusData> playableCharacterStatusDatas;
        public CharacterStatusData GetStatusData(PlayableCharacterType playableCharacterType)
            => playableCharacterStatusDatas.Single(x => x.PlayableCharacterType==playableCharacterType).CharacterStatusData;
    }
}

