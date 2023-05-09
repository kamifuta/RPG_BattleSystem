using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterBuff
    {
        public bool IsDefencing { get; private set; }

        public void SetIsDefencing(bool value)
        {
            IsDefencing = value;
        }
    }
}

