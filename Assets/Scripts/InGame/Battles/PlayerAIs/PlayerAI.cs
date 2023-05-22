using InGame.Skills;
using InGame.Parties;
using MyUtil;
using VContainer;
using InGame.Buttles.Actions;
using InGame.Items;
using UnityEngine;
using InGame.Magics;

namespace InGame.Buttles.PlayerAIs
{
    public class PlayerAI
    {
        private PartyManager partyManager;
        private EnemyManager enemyManager;
        private PlayableCharacterActionManager playableCharacterActionManager;

        [Inject]
        public PlayerAI(PartyManager partyManager)
        {
            this.partyManager = partyManager;
        }

        public void Init(EnemyManager enemyManager, PlayableCharacterActionManager playableCharacterActionManager)
        {
            this.enemyManager = enemyManager;
            this.playableCharacterActionManager = playableCharacterActionManager;
        }

        public void SelectCharacterAction()
        {
            foreach(var character in partyManager.partyCharacters)
            {
                if (character.characterHealth.IsDead)
                    continue;

                var random = Random.value;
                //var target = enemyManager.enemies.RandomGet();
                //ActionData action = new ActionData(BaseActionType.UseMagic, character, character, MagicType.HealMagic);
                //playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                if (random < 0.55f)
                {
                    var target = enemyManager.enemies.RandomGet();
                    ActionData action = new ActionData(BaseActionType.NormalAttack, character, target);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                else 
                {
                    ActionData action = new ActionData(BaseActionType.UseMagic, character, character, MagicType.HealMagic);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                //else if (random < 0.7f)
                //{
                //    ActionData action = new ActionData(BaseActionType.UseItem, character, character, ItemType.Herb);
                //    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                //}
                //else if(random < 0.85f)
                //{
                //    var target = enemyManager.enemies.RandomGet();
                //    ActionData action = new ActionData(BaseActionType.UseSkill, character, target, SkillType.PowerAttack);
                //    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                //}
                //else
                //{
                //    var target = enemyManager.enemies.RandomGet();
                //    ActionData action = new ActionData(BaseActionType.UseSkill, character, target, SkillType.QuickAttack);
                //    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                //}
            }
        }
    }
}

