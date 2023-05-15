using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/CharacterStatusData", fileName ="CharacterStatusData")]
    public class CharacterStatusData : ScriptableObject
    {
        [SerializeField] private int maxHP;
        [SerializeField] private int maxMP;
        [SerializeField] private int attackValue;
        [SerializeField] private int magicValue;
        [SerializeField] private int defenceValue;
        [SerializeField] private int magicDefenceValue;
        [SerializeField] private int agility;

        public int MaxHP => maxHP;
        public int MaxMP => maxMP;
        public int AttackValue => attackValue;
        public int MagicValue => magicValue;
        public int DefenceValue => defenceValue;
        public int MagicDefenceValue => magicDefenceValue;
        public int Agility => agility;
    }
}

