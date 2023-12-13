using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Skills
{
    public static class SkillDataBase
    {
        private static readonly List<SkillData> skillDataList = new List<SkillData>()
        {
            new PowerAttack(),
            new MowDownAttack(),
            new AttackDownAttack(),
            new DefenceDownAttack(),
            new AgilityDownAttack(),
            new MagicDownAttack(),
            new MagicDefenceDownAttack(),
        };

        public static IEnumerable<SkillData> GetUsableSkills(IEnumerable<SkillType> skillTypes, int currentMP)
            => skillDataList.Where(x => skillTypes.Any(y => x.skillType == y)).Where(x => x.consumeMP <= currentMP);

        public static SkillData GetSkillData(SkillType skillType)
            => skillDataList.Single(x => x.skillType == skillType);
    }
}

