using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Agents.Players
{
    public class PlayerAgentFactory : MonoBehaviour
    {
        [SerializeField] private PlayerAgent playerAgentPrefab;

        public PlayerAgent GeneratePlayerAgent(PlayableCharacter playableCharacter)
        {
            var agent = Instantiate(playerAgentPrefab);
            agent.SetAgentCharacter(playableCharacter);
            return agent;
        }

        public void DestroyPlayerAgent(PlayerAgent playerAgent)
        {
            if (playerAgent == null)
                return;

            Destroy(playerAgent.gameObject);
        }
    }
}
