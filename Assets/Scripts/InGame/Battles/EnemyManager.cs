using InGame.Buttles.EnemyAIs;
using InGame.Characters.Enemies;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class EnemyManager
    {
        public EnemyAI[] enemyAIs { get; private set; }
        public EnemyCharacter[] enemies { get; private set; }

        private readonly EnemyFactory enemyFactory;

        public EnemyManager(EnemyFactory enemyFactory)
        {
            this.enemyFactory = enemyFactory;
        }

        public void GenerateEnemies(EnemyType encountedEnemyType, int enemyAmount)
        {
            enemyAIs = new EnemyAI[enemyAmount];
            enemies = new EnemyCharacter[enemyAmount];
            for(int i = 0; i < enemyAmount; i++)
            {
                enemies[i] = enemyFactory.CreateEnemyCharacter(encountedEnemyType);
                enemyAIs[i] = enemyFactory.CreateEnemyAI(enemies[i]);
            }
        }

        public EnemyAI GetEnemyAI(EnemyCharacter enemyCharacter)
            => enemyAIs.Single(x => x.targetEnemy == enemyCharacter);
    }
}

