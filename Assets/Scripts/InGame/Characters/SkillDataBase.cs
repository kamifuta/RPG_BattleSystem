using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Characters.Skills
{
    public static class SkillDataBase
    {
        private static readonly List<SkillData> skillDataList = new List<SkillData>()
        {
            new SkillData(SkillType.NormalAttack, "攻撃", "普通の攻撃", 0, TargetType.Enemy, 0, false, SkillFunctions.NormalAttack),
            new SkillData(SkillType.Defence, "防御", "防御を二倍にする", 0, TargetType.Self, 1, false, SkillFunctions.Defence),
            new SkillData(SkillType.UseItem, "アイテムを使う", "アイテムを使う", 0, TargetType.Self, 0, false, SkillFunctions.Defence),
        };

        public static IEnumerable<SkillData> GetUsableSkills(IEnumerable<SkillType> skillTypes, int currentMP)
            => skillDataList.Where(x => skillTypes.Any(y => x.skillType == y)).Where(x => x.consumeMP <= currentMP);

        public static SkillData GetSkillData(SkillType skillType)
            => skillDataList.Single(x => x.skillType == skillType);
    }
}

