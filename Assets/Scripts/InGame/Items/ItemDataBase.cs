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
        private static readonly List<Item> itemList = new List<Item>()
        {
            new Herb(),
            new MagicPotion(),
        };

        public static Item GetItemData(ItemType itemType)
            => itemList.Single(x => x.itemType == itemType);
    }
}

