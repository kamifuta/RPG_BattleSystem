using InGame.Characters.Skills;
using InGame.Parties;
using MyUtil;
using VContainer;

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
                if (random < 0.7f)
                {
                    var target = enemyManager.enemies.RandomGet();
                    ActionData action = new ActionData(SkillDataBase.GetSkillData(SkillType.NormalAttack), character, target);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
                else
                {
                    ActionData action = new ActionData(SkillDataBase.GetSkillData(SkillType.Defence), character, character);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                }
            }
        }
    }
}

