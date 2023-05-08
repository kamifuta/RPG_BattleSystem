using InGame.Characters.Enemies;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class EnemyManager
    {
        public EnemyCharacter[] enemies { get; private set; }

        public void SetEnemies(IEnumerable<EnemyCharacter> enemies)
        {
            this.enemies = enemies.ToArray();
        }
    }
}

