using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCGs
{
    public class Party
    {
        public PlayableCharacter[] partyCharacters { get; private set; }
        public float winningParcentage { get; private set; }

        public Party(PlayableCharacter[] partyCharacters)
        {
            this.partyCharacters = partyCharacters;
        }

        public void SetWinningParcentage(float winningParcentage)
        {
            this.winningParcentage = winningParcentage;
        }
    }
}

