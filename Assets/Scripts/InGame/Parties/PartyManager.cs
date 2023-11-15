using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using MyUtil;
using System.Threading;

namespace InGame.Parties
{
    public class PartyManager
    {
        public PlayableCharacter[] partyCharacters { get; private set; } = new PlayableCharacter[4];

        public PartyManager(PlayableCharacter[] partyCharacters)
        {
            this.partyCharacters = partyCharacters;
        }
    }
}

