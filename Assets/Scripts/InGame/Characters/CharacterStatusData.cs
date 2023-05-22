using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/CharacterStatusData", fileName ="CharacterStatusData")]
    public class CharacterStatusData : ScriptableObject
    {
        [SerializeField] private int maxHP_min;
        [SerializeField] private int maxHP_max;
        [SerializeField] private int maxMP_min;
        [SerializeField] private int maxMP_max;
        [SerializeField] private int attackValue_min;
        [SerializeField] private int attackValue_max;
        [SerializeField] private int magicValue_min;
        [SerializeField] private int magicValue_max;
        [SerializeField] private int defenceValue_min;
        [SerializeField] private int defenceValue_max;
        [SerializeField] private int magicDefenceValue_min;
        [SerializeField] private int magicDefenceValue_max;
        [SerializeField] private int agility_min;
        [SerializeField] private int agility_max;

        public int MaxHP => Random.Range(maxHP_min, maxHP_max + 1);
        public int MaxMP => Random.Range(maxMP_min, maxMP_max + 1);
        public int AttackValue => Random.Range(attackValue_min, attackValue_max + 1);
        public int MagicValue => Random.Range(magicValue_min, magicValue_max + 1);
        public int DefenceValue => Random.Range(defenceValue_min, defenceValue_max + 1);
        public int MagicDefenceValue => Random.Range(magicDefenceValue_min, magicDefenceValue_max + 1);
        public int Agility => Random.Range(agility_min, agility_max + 1);
    }
}

