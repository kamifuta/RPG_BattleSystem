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
        [SerializeField] private int defenceValue;
        [SerializeField] private int agility;

        public int MaxHP => maxHP;
        public int MaxMP => maxMP;
        public int AttackValue => attackValue;
        public int DefenceValue => defenceValue;
        public int Agility => agility;
    }
}

