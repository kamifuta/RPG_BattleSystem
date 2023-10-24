using InGame.Buttles.EnemyAIs;
using InGame.Characters;
using InGame.Characters.Enemies;
using InGame.Characters.PlayableCharacters;
using InGame.Parties;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Buttles
{
    public class EnemyFactory
    {
        private EnemyStatusDataTable enemyStatusDataTable;

        public EnemyFactory(EnemyStatusDataTable enemyStatusDataTable)
        {
            this.enemyStatusDataTable = enemyStatusDataTable;
        }

        public EnemyCharacter CreateEnemyCharacter(EnemyType enemyType)
        {
            var statusData = enemyStatusDataTable.GetStatusData(enemyType);
            var status = new CharacterStatus(statusData);
            var enemy = new EnemyCharacter(status, statusData.UsableSkillList, statusData.UsableMagicList);
            return enemy;
        }

        public EnemyAI CreateEnemyAI(EnemyCharacter target, IEnumerable<PlayableCharacter> playableCharacters)
        {
            var enemyAI = new EnemyAI(target, playableCharacters);
            return enemyAI;
        }
    }
}

