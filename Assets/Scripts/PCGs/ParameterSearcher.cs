using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtil;
using System.Linq;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;

namespace PCGs
{
    public class ParameterSearcher
    {
        private CharacterManager characterManager;
        private EvaluationFunctions evaluationFunctions;

        private readonly int searchTimes = 5000;
        private readonly int battleTimes = 10;

        public void StartSearch()
        {
            characterManager.GenerateCharacters();
            for(int i = 0; i < searchTimes; i++)
            {
                var character = characterManager.playableCharacters.RandomGet();
                var parties = characterManager.GetAllParty().Where(x => x.partyCharacters.Any(y => y == character));
                foreach (var party in parties)
                {
                    for (int j = 0; j < battleTimes; j++)
                    {
                        //バトルを実行する
                    }
                }

                var synergyPoint = evaluationFunctions.EvaluateSynergy(parties);
                var evaluation = evaluationFunctions.EvaluateCharacter(synergyPoint);

                var variantHP = Mathf.CeilToInt(character.characterStatus.baseMaxHP * Random.Range(0.9f, 1.1f));
                var variantMP = Mathf.CeilToInt(character.characterStatus.baseMaxMP * Random.Range(0.9f, 1.1f));
                var variantAttack = Mathf.CeilToInt(character.characterStatus.baseAttackValue * Random.Range(0.9f, 1.1f));
                var variantMagic = Mathf.CeilToInt(character.characterStatus.baseMagicValue * Random.Range(0.9f, 1.1f));
                var variantDefence = Mathf.CeilToInt(character.characterStatus.baseDefenceValue * Random.Range(0.9f, 1.1f));
                var variantMagicDefence = Mathf.CeilToInt(character.characterStatus.baseMagicDefenceValue * Random.Range(0.9f, 1.1f));
                var variantAgility = Mathf.CeilToInt(character.characterStatus.baseAgility * Random.Range(0.9f, 1.1f));
                var variantStatus = new CharacterStatus(variantHP, variantMP, variantAttack, variantMagic, variantDefence, variantMagicDefence, variantAgility);

                var variantCharacter = new PlayableCharacter(variantStatus);
                characterManager.playableCharacters.Add(variantCharacter);
                var variantParties = characterManager.GetAllParty().Where(x => x.partyCharacters.Any(y => y == variantCharacter) && !x.partyCharacters.Any(y => y == character));
                foreach (var party in variantParties)
                {
                    for (int j = 0; j < battleTimes; j++)
                    {
                        //バトルを実行する
                    }
                }

                var variantSynergyPoint = evaluationFunctions.EvaluateSynergy(variantParties);
                var variantEvaluation = evaluationFunctions.EvaluateCharacter(variantSynergyPoint);

                if (variantEvaluation > evaluation)
                {
                    characterManager.playableCharacters.Remove(character);
                }
                else
                {
                    characterManager.playableCharacters.Remove(variantCharacter);
                }
            }
        }
    }
}
