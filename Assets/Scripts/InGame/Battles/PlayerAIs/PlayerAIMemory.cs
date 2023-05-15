using InGame.Characters.Enemies;
using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAIMemory
    {
        private EnemyCharacter targetedEnemy;

        private List<EnemyCharacter> dyingEnemy;
        private List<EnemyCharacter> notFullHealthEnemy;
        private List<PlayableCharacter> dyingPlayer;
    }
}

