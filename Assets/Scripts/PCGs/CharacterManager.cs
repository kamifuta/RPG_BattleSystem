using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Items;
using InGame.Magics;
using InGame.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyUtil;

namespace PCGs
{
    public class CharacterManager
    {
        public List<PlayableCharacter> playableCharacters { get; private set; }
        private CharacterStatusData characterStatusData;

        public CharacterManager(CharacterStatusData characterStatusData)
        {
            this.characterStatusData = characterStatusData;
        }

        public void GenerateCharacters(int amount)
        {
            playableCharacters = new List<PlayableCharacter>();

            for(int i = 0; i < amount; i++)
            {
                var status = new CharacterStatus(characterStatusData);
                var character = new PlayableCharacter(status);
                character.SetCharacterName("Player_" + (char)('A' + i));
                
                playableCharacters.Add(character);

                foreach(var skill in Enum.GetValues(typeof(SkillType)))
                {
                    character.AddSkill((SkillType)skill);
                }

                foreach(var magic in Enum.GetValues(typeof(MagicType)))
                {
                    character.AddMagic((MagicType)magic);
                }

                var randam = new System.Random();
                for (int j = 0; j < 3; j++)
                {
                    var enumValues = Enum.GetValues(typeof(ItemType));
                    var itemType = (ItemType)enumValues.GetValue(randam.Next(enumValues.Length));
                    character.AddItem(itemType);
                }
            }
        }

        public void SetItems()
        {
            foreach(var character in playableCharacters)
            {
                character.CleanItems();
                var randam = new System.Random();
                for (int j = 0; j < 3; j++)
                {
                    var enumValues = Enum.GetValues(typeof(ItemType));
                    var itemType = (ItemType)enumValues.GetValue(randam.Next(enumValues.Length));
                    character.AddItem(itemType);
                }
            }
        }

        public IEnumerable<Party> GetAllParties()
        {
            var partMemberList = playableCharacters.Combination(4);
            foreach(var partyMember in partMemberList)
            {
                var party = new Party(partyMember.ToArray());
                yield return party;
            }
        }

        public IEnumerable<Party> GetParties(PlayableCharacter containCharacter)
        {
            var partMemberList = playableCharacters.Combination(containCharacter, 4);
            foreach (var partyMember in partMemberList)
            {
                var party = new Party(partyMember.ToArray());
                yield return party;
            }
        }

        public IEnumerable<Party> GetParties(PlayableCharacter containCharacter, IEnumerable<PlayableCharacter> exceptCharcters)
        {
            var partMemberList = playableCharacters.Combination(containCharacter, exceptCharcters, 4);
            foreach (var partyMember in partMemberList)
            {
                var party = new Party(partyMember.ToArray());
                yield return party;
            }
        }
    }
}
