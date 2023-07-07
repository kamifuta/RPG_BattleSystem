using InGame.Buttles.EnemyAIs;
using InGame.Characters.Enemies;
using InGame.Parties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace InGame.Buttles
{
    public class EnemyManager : IDisposable
    {
        public EnemyAI[] enemyAIs { get; private set; }
        public EnemyCharacter[] enemies { get; private set; }

        private readonly EnemyFactory enemyFactory;
        private readonly PartyManager partyManager;

        public bool HadDisposed { get; private set; } = false;

        public EnemyManager(EnemyFactory enemyFactory, PartyManager partyManager)
        {
            this.enemyFactory = enemyFactory;
            this.partyManager = partyManager;
        }

        public void GenerateEnemies(EnemyType encountedEnemyType, int enemyAmount)
        {
            enemyAIs = new EnemyAI[enemyAmount];
            enemies = new EnemyCharacter[enemyAmount];
            for(int i = 0; i < enemyAmount; i++)
            {
                enemies[i] = enemyFactory.CreateEnemyCharacter(encountedEnemyType);
                enemyAIs[i] = enemyFactory.CreateEnemyAI(enemies[i], partyManager.partyCharacters);

                enemies[i].SetCharacterName($"{Enum.GetName(typeof(EnemyType), encountedEnemyType)}_{(char)('A' + i)}");
            }
            //Debug.Log($"<color=red>generated enemy</color>{Thread.CurrentThread.ManagedThreadId}");
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
            HadDisposed = true;
            Debug.Log("EnemyManagerは破棄されました");
        }
    }
}

