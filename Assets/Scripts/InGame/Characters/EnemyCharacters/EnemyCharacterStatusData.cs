using InGame.Magics;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters.Enemies
{
    [CreateAssetMenu(menuName = "MyScriptable/EnemyCharacterStatusData", fileName = "CharacterStatusData")]
    public class EnemyCharacterStatusData : CharacterStatusData
    {
        [SerializeField] private List<SkillType> usableSkillList;
        [SerializeField] private List<MagicType> usableMagicList;

        public List<SkillType> UsableSkillList => usableSkillList;
        public List<MagicType> UsableMagicList => usableMagicList;
    }
}

