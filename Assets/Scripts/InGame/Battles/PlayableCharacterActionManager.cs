using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Characters.PlayableCharacters;
using System;
using InGame.Characters;
using System.Linq;
using InGame.Skills;
using InGame.Buttles.Actions;

namespace InGame.Buttles
{
    public class PlayableCharacterActionManager
    {
        private readonly Dictionary<PlayableCharacter, ActionData> playableCharacterActionDic = new Dictionary<PlayableCharacter, ActionData>();

        public void SetPlayableCharacterAction(PlayableCharacter playableCharacter, ActionData action)
        {
            playableCharacterActionDic.Add(playableCharacter, action);
        }

        public void ClearDic()
        {
            playableCharacterActionDic.Clear();
        }

        public KeyValuePair<PlayableCharacter, ActionData> GetCharacterActionPair(PlayableCharacter playableCharacter)
            => playableCharacterActionDic.Single(x => x.Key == playableCharacter);

        public ActionData GetCharacterActionData(PlayableCharacter playableCharacter)
            => playableCharacterActionDic.Single(x => x.Key == playableCharacter).Value;

        public IEnumerable<KeyValuePair<PlayableCharacter, ActionData>> GetDefenceActionPairs()
            => playableCharacterActionDic.Where(x => x.Value.actionType == BaseActionType.Defence);

        public IEnumerable<PlayableCharacter> GetHighPriorityActionCharacters()
            => playableCharacterActionDic
                .Where(x => x.Value.actionType == BaseActionType.UseSkill)
                .Where(x => SkillDataBase.GetSkillData(x.Value.skillType).priority > 0)
                .Select(t => t.Key);

        public IEnumerable<PlayableCharacter> GetNormalPriorityActionCharacters()
        {
            var normalAttackPlayer = playableCharacterActionDic.Where(x => x.Value.actionType == BaseActionType.NormalAttack).Select(x=>x.Key);
            var useItemPlayer = playableCharacterActionDic.Where(x => x.Value.actionType == BaseActionType.UseItem).Select(x=>x.Key);
            var useMagicPlayer = playableCharacterActionDic.Where(x => x.Value.actionType == BaseActionType.UseMagic).Select(x=>x.Key);
            var normalPriorityActionPlayer= playableCharacterActionDic
                                                .Where(x => x.Value.actionType == BaseActionType.UseSkill)
                                                .Where(x => SkillDataBase.GetSkillData(x.Value.skillType).priority == 0).Select(x => x.Key);
            
            return normalAttackPlayer.Union(useItemPlayer).Union(useMagicPlayer).Union(normalPriorityActionPlayer);
        }

        public IEnumerable<PlayableCharacter> GetLowPriorityActionCharacters()
            => playableCharacterActionDic
                .Where(x => x.Value.actionType == BaseActionType.UseSkill)
                .Where(x => SkillDataBase.GetSkillData(x.Value.skillType).priority < 0)
                .Select(t => t.Key);
    }
}

