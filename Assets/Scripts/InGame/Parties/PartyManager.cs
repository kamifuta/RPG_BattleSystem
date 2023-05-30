using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using MyUtil;

namespace InGame.Parties
{
    public class PartyManager
    {
        public PlayableCharacter[] partyCharacters { get; private set; } = new PlayableCharacter[4];
        private PlayableCharacterStatusDataTable statusDataTable;

        [Inject]
        public PartyManager(PlayableCharacterStatusDataTable statusDataTable)
        {
            this.statusDataTable = statusDataTable;
            InitParty();

            //Addressables.LoadAssetAsync<PlayableCharacterStatusDataTable>("PlayableCharacterStatusDataTable").Completed += handle =>
            //{
            //    statusDataTable = handle.Result;

            //    partyCharacters[0] = CreatePlayableCharacter(PlayableCharacterType.Warrior);
            //    partyCharacters[0].SetCharacterName($"êÌém");

            //    partyCharacters[1] = CreatePlayableCharacter(PlayableCharacterType.Priest);
            //    partyCharacters[1].SetCharacterName($"ëmóµ");

            //    partyCharacters[2] = CreatePlayableCharacter(PlayableCharacterType.Mage);
            //    partyCharacters[2].SetCharacterName($"ñÇñ@égÇ¢");

            //    partyCharacters[3] = CreatePlayableCharacter(PlayableCharacterType.MartialArtist);
            //    partyCharacters[3].SetCharacterName($"ïêì¨â∆");
            //};
        }

        private PlayableCharacter CreatePlayableCharacter(PlayableCharacterType type)
        {
            var statusData = statusDataTable.GetStatusData(type);
            var status = new CharacterStatus(statusData);

            var character= new PlayableCharacter(status);

            var skillList = statusDataTable.GetSkillList(type);
            foreach(var skill in skillList.MustSkills)
            {
                character.AddSkill(skill);
            }

            foreach (var skill in skillList.Skills)
            {
                if (UnityEngine.Random.value < 0.5f)
                    continue;
                character.AddSkill(skill);
            }

            foreach (var magic in skillList.MustMagics)
            {
                character.AddMagic(magic);
            }

            foreach (var magic in skillList.Magics)
            {
                if (UnityEngine.Random.value < 0.5f)
                    continue;
                character.AddMagic(magic);
            }

            var randam = new System.Random();
            for(int i = 0; i < 3; i++)
            {
                var enumValues = Enum.GetValues(typeof(ItemType));
                var itemType = (ItemType)enumValues.GetValue(randam.Next(enumValues.Length));
                character.AddItem(itemType);
            }

            return character;
        }

        public void InitParty()
        {
            partyCharacters[0] = CreatePlayableCharacter(PlayableCharacterType.Warrior);
            partyCharacters[0].SetCharacterName($"êÌém");

            partyCharacters[1] = CreatePlayableCharacter(PlayableCharacterType.Priest);
            partyCharacters[1].SetCharacterName($"ëmóµ");

            partyCharacters[2] = CreatePlayableCharacter(PlayableCharacterType.Mage);
            partyCharacters[2].SetCharacterName($"ñÇñ@égÇ¢");

            partyCharacters[3] = CreatePlayableCharacter(PlayableCharacterType.MartialArtist);
            partyCharacters[3].SetCharacterName($"ïêì¨â∆");
        }
    }
}

