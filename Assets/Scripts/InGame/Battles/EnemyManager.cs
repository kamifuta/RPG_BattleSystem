using InGame.Buttles.EnemyAIs;
using InGame.Characters.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class EnemyManager : IDisposable
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

                enemies[i].SetCharacterName($"{Enum.GetName(typeof(EnemyType), encountedEnemyType)}_{(char)('A' + i)}");
            }
        }

        public EnemyAI GetEnemyAI(EnemyCharacter enemyCharacter)
            => enemyAIs.Single(x => x.targetEnemy == enemyCharacter);

        public void Dispose()
        {
            foreach(var AI in enemyAIs)
            {
                AI.Dispose();
            }

            enemyAIs = Enumerable.Empty<EnemyAI>().ToArray();
            Debug.Log("EnemyManager‚Í”jŠü‚³‚ê‚Ü‚µ‚½");
        }
    }
}

