using InGame.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters.PlayableCharacters
{
    public class PlayableCharacter : BaseCharacter
    {
        private List<ItemType> haveItemList = new List<ItemType>();
        public IReadOnlyList<ItemType> HaveItemList => haveItemList;

        public PlayableCharacter(CharacterStatus characterStatus) : base(characterStatus)
        {

        }

        public void AddItem(ItemType itemType)
        {
            haveItemList.Add(itemType);
        }

        public void RemoveItem(ItemType itemType)
        {
            haveItemList.Remove(itemType);
        }

        public void CleanItems()
        {
            haveItemList.Clear();
        }
    }
}

