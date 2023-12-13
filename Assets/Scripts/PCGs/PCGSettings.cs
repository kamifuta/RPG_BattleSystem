using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCGs
{
    [CreateAssetMenu(menuName ="MyScriptable/PCGSettings", fileName ="PCGSettings")]
    public class PCGSettings : ScriptableObject
    {
        [SerializeField] private int alpha;
        [SerializeField] private int beta;
        [SerializeField] private int gamma;
        [SerializeField] private int delta;

        [SerializeField] private EnemyStatusDataTable enemyStatusDataTable;

        [SerializeField] private CharacterStatusData statusData;
        [SerializeField] private int splitCount;

        [SerializeField] private int simulateTurn;

        public int Alpha => alpha;
        public int Beta => beta;
        public int Gamma => gamma;
        public int Delta => delta;

        public EnemyStatusDataTable EnemyStatusDataTable => enemyStatusDataTable;

        public CharacterStatusData StatusData => statusData;
        public int SplitCount => splitCount;

        public int SimulateTurn => simulateTurn;
    }
}

