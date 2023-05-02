using InGame.Characters.Enemies;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class ButtleManager
    {
        private EnemyManager enemyManager;
        private EmargingEnemiesList EmargingEnemiesList;
        private EnemyFactory enemyFactory;

        public void StartButtle(EnemyType encountedEnemyType, int enemyAmount)
        {
            GenerateEnemies(encountedEnemyType, enemyAmount);
        }

        private void GenerateEnemies(EnemyType encountedEnemyType, int enemyAmount)
        {
            Enemy[] enemies = new Enemy[enemyAmount];
            var mainEnemyAmount = Random.Range(1, enemyAmount + 1);

            for(int i = 0; i < mainEnemyAmount; i++)
            {
                enemies[i] = enemyFactory.CreateEnemy(encountedEnemyType);
            }

            if (mainEnemyAmount < enemyAmount)
            {
                var enemyType = EmargingEnemiesList.EnemyTypes.RandomGet();
                for (int i = mainEnemyAmount; i < enemyAmount; i++)
                {
                    enemies[i] = enemyFactory.CreateEnemy(enemyType);
                }
            }

            enemyManager.SetEnemies(enemies);
        }
    }
}

