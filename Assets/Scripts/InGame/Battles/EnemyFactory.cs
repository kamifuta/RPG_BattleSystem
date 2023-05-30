using InGame.Buttles.EnemyAIs;
using InGame.Characters;
using InGame.Characters.Enemies;
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
        private PartyManager partyManager;

        public EnemyFactory(PartyManager partyManager, EnemyStatusDataTable enemyStatusDataTable)
        {
            this.partyManager = partyManager;
            this.enemyStatusDataTable = enemyStatusDataTable;

            //Addressables.LoadAssetAsync<EnemyStatusDataTable>("EnemyStatusDataTable").Completed += hundler =>
            //{
            //    enemyStatusDataTable = hundler.Result;
            //};
        }

        public EnemyCharacter CreateEnemyCharacter(EnemyType enemyType)
        {
            var statusData = enemyStatusDataTable.GetStatusData(enemyType);
            var status = new CharacterStatus(statusData);
            var enemy = new EnemyCharacter(status);
            return enemy;
        }

        public EnemyAI CreateEnemyAI(EnemyCharacter target)
        {
            var enemyAI = new EnemyAI(target, partyManager);
            return enemyAI;
        }
    }
}

