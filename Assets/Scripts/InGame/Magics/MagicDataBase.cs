using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace InGame.Magics
{
    public class MagicDataBase
    {
        private static readonly List<MagicData> magicDataList = new List<MagicData>()
        {
            new MagicMissile(),
            new BigMagicMissile(),
            new HealMagic(),
            new RevivalMagic(),
            new BuffAttackMagic(),
            new BuffMagicMagic(),
            new BuffDefenceMagic(),
            new BuffMagicDefenceMagic(),
            new BuffAgilityMagic(),
            new DebuffAttackMagic(),
            new DebuffMagicMagic(),
            new DebuffDefenceMagic(),
            new DebuffMagicDefenceMagic(),
            new DebuffAgilityMagic(),
        };

        public static ReadOnlyCollection<MagicData> MagicDatas => new ReadOnlyCollection<MagicData>(magicDataList);

        public static IEnumerable<MagicData> GetUsableMagics(IEnumerable<MagicType> skillTypes, int currentMP)
            => magicDataList.Where(x => skillTypes.Any(y => x.magicType == y)).Where(x => x.consumeMP <= currentMP);

        public static MagicData GetMagicData(MagicType magicType)
            => magicDataList.Single(x => x.magicType == magicType);
    }
}

