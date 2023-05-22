using InGame.Characters.PlayableCharacters;
using InGame.Magics;
using InGame.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    [CreateAssetMenu(menuName ="MyScriptable/CharacterSkillList", fileName ="CharcterSkillList")]
    public class CharacterSkillList : ScriptableObject
    {
        [SerializeField] private List<SkillType> mustSkills;
        [SerializeField] private List<SkillType> skills;
        [SerializeField] private List<MagicType> mustMagics;
        [SerializeField] private List<MagicType> magics;

        public IEnumerable<SkillType> MustSkills => mustSkills;
        public IEnumerable<SkillType> Skills => skills;
        public IEnumerable<MagicType> MustMagics => mustMagics;
        public IEnumerable<MagicType> Magics => magics;
    }
}

