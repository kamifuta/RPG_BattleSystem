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
        private PlayableCharacterStatusDataTable statusDataTable;

        public PartyManager()
        {
            Addressables.LoadAssetAsync<PlayableCharacterStatusDataTable>("PlayableCharacterStatusDataTable").Completed += handle =>
            {
                statusDataTable = handle.Result;

                partyCharacters[0] = CreatePlayableCharacter(PlayableCharacterType.Warrior);
                partyCharacters[0].SetCharacterName($"ím");

                partyCharacters[1] = CreatePlayableCharacter(PlayableCharacterType.Priest);
                partyCharacters[1].SetCharacterName($"‘m—µ");

                partyCharacters[2] = CreatePlayableCharacter(PlayableCharacterType.Mage);
                partyCharacters[2].SetCharacterName($"–‚–@g‚¢");

                partyCharacters[3] = CreatePlayableCharacter(PlayableCharacterType.MartialArtist);
                partyCharacters[3].SetCharacterName($"•“¬‰Æ");
            };
        }

        private PlayableCharacter CreatePlayableCharacter(PlayableCharacterType type)
        {
            var statusData = statusDataTable.GetStatusData(type);
            var status = new CharacterStatus(statusData);
            return new PlayableCharacter(status);
        }
    }
}

