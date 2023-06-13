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

namespace PCGs
{
    public class CharacterManager
    {
        public List<PlayableCharacter> playableCharacters { get; private set; }

        //public IEnumerable<Party> parties;

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

        //public void ControlParameter()
        //{
        //    parties = GetAllParty();
        //}

        public IEnumerable<Party> GetAllParty()
        {
            var partMemberList = Enumerate<PlayableCharacter>(playableCharacters, 4, false);
            foreach(var partyMember in partMemberList)
            {
                var party = new Party(partyMember);
                yield return party;
            }
        }

        private IEnumerable<T[]> Enumerate<T>(IEnumerable<T> items, int k, bool withRepetition)
        {
            if (k == 1)
            {
                foreach (var item in items)
                    yield return new T[] { item };
                yield break;
            }
            foreach (var item in items)
            {
                var leftside = new T[] { item };

                // item よりも前のものを除く （順列と組み合わせの違い)
                // 重複を許さないので、unusedから item そのものも取り除く
                var unused = withRepetition ? items : items.SkipWhile(e => !e.Equals(item)).Skip(1).ToList();

                foreach (var rightside in Enumerate(unused, k - 1, withRepetition))
                {
                    yield return leftside.Concat(rightside).ToArray();
                }
            }
        }
    }
}
