using InGame.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters.PlayableCharacters
{
    public class PlayableCharacter : BaseCharacter
    {
        private List<ItemType> havItemList = new List<ItemType>();
        public IReadOnlyList<ItemType> HaveItemList => havItemList;

        public PlayableCharacter(CharacterStatus characterStatus) : base(characterStatus)
        {

        }

        public void AddItem(ItemType itemType)
        {
            havItemList.Add(itemType);
        }

        public void RemoveItem(ItemType itemType)
        {
            havItemList.Remove(itemType);
        }
    }
}

