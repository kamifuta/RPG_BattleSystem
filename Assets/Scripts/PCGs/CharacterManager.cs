using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PCGs
{
    public class CharacterManager
    {
        public List<PlayableCharacter> playableCharacters { get; private set; }

        public IEnumerable<Party> parties;

        public void GenerateCharacters()
        {

        }

        public void ControlParameter()
        {
            parties = GetAllParty();
        }

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
