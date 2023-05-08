using InGame.Characters.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles
{
    [CreateAssetMenu(menuName ="MyScriptable/EmargingEnemiesList", fileName = "EmargingEnemiesList")]
    public class EmargingEnemiesList : ScriptableObject
    {
        [SerializeField] private List<EnemyType> enemyTypes;

        public IReadOnlyList<EnemyType> EnemyTypes => enemyTypes;
    }
}

