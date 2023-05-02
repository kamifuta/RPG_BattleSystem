using InGame.Characters.PlayableCharacters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Parties
{
    public class PartyManager : MonoBehaviour
    {
        public PlayableCharacter[] partyCharacters { get; private set; } = new PlayableCharacter[4];
    }
}

