using InGame.Characters.Enemies;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/EnemyStatusDataTable", fileName = "EnemyStatusDataTable")]
    public class EnemyStatusDataTable : ScriptableObject
    {
        [Serializable]
        public class EnemyStatusData
        {
            [SerializeField] private EnemyType enemyType;
            [SerializeField] private CharacterStatusData characterStatusData;

            public EnemyType EnemyType => enemyType;
            public CharacterStatusData CharacterStatusData => characterStatusData;
        }

        [SerializeField] private List<EnemyStatusData> enemyStatusDatas;
        public CharacterStatusData GetStatusData(EnemyType enemyType)
            => enemyStatusDatas.Single(x => x.EnemyType == enemyType).CharacterStatusData;
    }
}

