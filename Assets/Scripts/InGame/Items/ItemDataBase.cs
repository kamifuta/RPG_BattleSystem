using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Items
{
    public enum ItemType
    {
        Herb,
        MagicPotion,
    }

    public static class ItemDataBase
    {
        private static readonly List<ItemData> itemList = new List<ItemData>()
        {
            new Herb(),
            new MagicPotion(),
        };

        public static ItemData GetItemData(ItemType itemType)
            => itemList.Single(x => x.itemType == itemType);
    }
}

