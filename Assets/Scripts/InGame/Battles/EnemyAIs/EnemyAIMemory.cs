using InGame.Characters.PlayableCharacters;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAIMemory
    {
        private readonly Dictionary<PlayableCharacter, float> hateDic;
        public IEnumerable<PlayableCharacter> HateCharacters => hateDic.Keys;

        public PlayableCharacter TargetPlayer()
        {
            var total = hateDic.Where(t=>!t.Key.characterHealth.IsDead).Select(t => t.Value).Sum();
            var rand = Random.value * total;
            foreach(var character in hateDic.Keys)
            {
                if (character.characterHealth.IsDead)
                    continue;

                if (rand < hateDic[character])
                {
                    return character;
                }
                else
                {
                    total -= hateDic[character];
                }
            }

            return hateDic.Keys.RandomGet();
        }

        public EnemyAIMemory(IEnumerable<PlayableCharacter> partyCharacters)
        {
            hateDic = partyCharacters.ToDictionary(x => x, _ => 0.01f);
        }

        public void AddHate(PlayableCharacter player, float hateValue)
        {
            hateDic[player] += hateValue;
        }
    }
}

