using InGame.Characters.Skills;
using InGame.Parties;
using MyUtil;
using VContainer;
using InGame.Buttles.Actions;
using InGame.Items;

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

                var random = UnityEngine.Random.value;
                if (random < 0.5f)
                {
                    var target = enemyManager.enemies.RandomGet();
                    var arg = new ActionArgument(character, target);
                    ActionData action = new ActionData(SkillType.NormalAttack, arg);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                else if(random < 0.7f)
                {
                    var arg = new ActionArgument(character, character);
                    ActionData action = new ActionData(SkillType.Defence, arg);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                else
                {
                    var arg = new UseItemActionArgument(character, character, ItemType.Herb);
                    ActionData action = new ActionData(SkillType.UseItem, arg);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
            }
        }
    }
}

