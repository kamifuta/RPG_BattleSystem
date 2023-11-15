using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/CharacterStatusData", fileName ="CharacterStatusData")]
    public class CharacterStatusData : ScriptableObject
    {
        [SerializeField] public int maxHP_min;
        [SerializeField] public int maxHP_max;
        [SerializeField] public int maxMP_min;
        [SerializeField] public int maxMP_max;
        [SerializeField] public int attackValue_min;
        [SerializeField] public int attackValue_max;
        [SerializeField] public int magicValue_min;
        [SerializeField] public int magicValue_max;
        [SerializeField] public int defenceValue_min;
        [SerializeField] public int defenceValue_max;
        [SerializeField] public int magicDefenceValue_min;
        [SerializeField] public int magicDefenceValue_max;
        [SerializeField] public int agility_min;
        [SerializeField] public int agility_max;

        public int MaxHP => Random.Range(maxHP_min, maxHP_max + 1);
        public int MaxMP => Random.Range(maxMP_min, maxMP_max + 1);
        public int AttackValue => Random.Range(attackValue_min, attackValue_max + 1);
        public int MagicValue => Random.Range(magicValue_min, magicValue_max + 1);
        public int DefenceValue => Random.Range(defenceValue_min, defenceValue_max + 1);
        public int MagicDefenceValue => Random.Range(magicDefenceValue_min, magicDefenceValue_max + 1);
        public int Agility => Random.Range(agility_min, agility_max + 1);
    }
}

