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
        public bool IsSimulated { get; private set; }
        public bool HadSuspended { get; private set; }

        public Party(PlayableCharacter[] partyCharacters)
        {
            this.partyCharacters = partyCharacters;
        }

        public void SetWinningParcentage(float winningParcentage)
        {
            this.winningParcentage = winningParcentage;
        }

        public void SetIsSimulated(bool value)
        {
            IsSimulated = value;
        }

        public void SetHadSuspended(bool value)
        {
            HadSuspended = value;
        }
    }
}

