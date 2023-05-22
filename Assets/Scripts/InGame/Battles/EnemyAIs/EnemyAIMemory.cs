using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles.EnemyAIs
{
    public class EnemyAIMemory
    {
        private readonly Dictionary<PlayableCharacter, float> hateDic;

        public PlayableCharacter TargetPlayer => hateDic.Where(t=>!t.Key.characterHealth.IsDead).OrderByDescending(x => x.Value).First().Key;

        public EnemyAIMemory(IEnumerable<PlayableCharacter> partyCharacters)
        {
            hateDic = partyCharacters.ToDictionary(x => x, _ => 0f);
        }

        public void AddHate(PlayableCharacter player, float hateValue)
        {
            hateDic[player] += hateValue;
        }
    }
}

