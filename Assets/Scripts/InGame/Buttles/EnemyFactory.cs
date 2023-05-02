using InGame.Characters;
using InGame.Characters.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Buttles
{
    public class EnemyFactory
    {
        private readonly EnemyStatusDataTable enemyStatusDataTable;

        public EnemyFactory()
        {
            enemyStatusDataTable = Addressables.LoadAssetAsync<EnemyStatusDataTable>("EnemyStatusDataTable").Result;
        }

        public Enemy CreateEnemy(EnemyType enemyType)
        {
            var statusData = enemyStatusDataTable.GetStatusData(enemyType);
            var status = new CharacterStatus(statusData);
            var enemy = new Enemy(status);
            return enemy;
        }
    }
}

