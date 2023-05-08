using InGame.Characters.Enemies;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class BattleManager
    {
        private EnemyManager enemyManager;
        
        public void StartButtle(EnemyType encountedEnemyType)
        {
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        public void StartButtle(EnemyType encountedEnemyType, int enemyAmount)
        {
            enemyManager.GenerateEnemies(encountedEnemyType, enemyAmount);
        }

        public void StartButtle(EnemyType encountedEnemyType, int minEnemyAmount, int maxEnemyAmount)
        {
            int enemyAmount = Random.Range(minEnemyAmount, maxEnemyAmount+1);
            StartButtle(encountedEnemyType, enemyAmount);
        }

        public void StartButtle(IEnumerable<EnemyType> encountedEnemyTypes)
        {
            //TODO: ‚·‚×‚Ä‚Ì¶¬ƒLƒƒƒ‰‚ğw’è‚·‚éê‡‚ÌŠÖ”‚ğì¬‚·‚é
        }
    }
}

