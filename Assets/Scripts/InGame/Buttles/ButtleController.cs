using InGame.Characters;
using InGame.Parties;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class ButtleController
    {
        private EnemyManager enemyManager;
        private PartyManager partyManager;

        public void GenerateEnemies()
        {

        }

        public void ProcessButtle()
        {

        }

        private IEnumerable<BaseCharacter> GetSortedCharacterByAgility()
            => enemyManager.enemies.Cast<BaseCharacter>()
                .Concat(partyManager.partyCharacters)
                .OrderBy(x => x.characterStatus.Agility);
    }
}

