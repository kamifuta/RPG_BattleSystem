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
            new HealMagic(),
            new RevivalMagic(),
        };

        public static ReadOnlyCollection<MagicData> MagicDatas => new ReadOnlyCollection<MagicData>(magicDataList);

        public static IEnumerable<MagicData> GetUsableMagics(IEnumerable<MagicType> skillTypes, int currentMP)
            => magicDataList.Where(x => skillTypes.Any(y => x.magicType == y)).Where(x => x.consumeMP <= currentMP);

        public static MagicData GetMagicData(MagicType skillType)
            => magicDataList.Single(x => x.magicType == skillType);
    }
}

