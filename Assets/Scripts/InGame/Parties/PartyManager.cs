using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Parties
{
    public class PartyManager
    {
        public PlayableCharacter[] partyCharacters { get; private set; } = new PlayableCharacter[4];

        public PartyManager()
        {
            Addressables.LoadAssetAsync<PlayableCharacterStatusDataTable>("PlayableCharacterStatusDataTable").Completed += result =>
            {
                for (int i = 0; i < 4; i++)
                {
                    var statusData = result.Result.GetStatusData(PlayableCharacterType.Test);
                    var status = new CharacterStatus(statusData);
                    partyCharacters[i] = new PlayableCharacter(status);
                }
            };
        }
    }
}

