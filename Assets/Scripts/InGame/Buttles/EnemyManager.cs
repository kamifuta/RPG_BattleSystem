using InGame.Characters.Enemies;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles
{
    public class EnemyManager
    {
        public Enemy[] enemies { get; private set; }

        public void SetEnemies(IEnumerable<Enemy> enemies)
        {
            this.enemies = enemies.ToArray();
        }
    }
}

