using InGame.Characters;
using InGame.Fields;
using InGame.Parties;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using InGame.Characters.Enemies;
using InGame.Buttles.PlayerAIs;
using InGame.Characters.PlayableCharacters;
using MyUtil;
using VContainer.Unity;
using VContainer;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;

namespace InGame.Buttles
{
    public class BattleController : ControllerBase, IStartable
    {
        private enum ResultType
        {
            None,
            Win,
            Lose,
        }

        private EnemyManager enemyManager;
        private TurnManager turnManager = new TurnManager();
        private PlayableCharacterActionManager playableCharacterActionManager = new PlayableCharacterActionManager();

        private PartyManager partyManager;
        private FieldManager fieldManager;
        private PlayerAI playerAI;

        [Inject]
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAI playerAI)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            this.playerAI = playerAI;

            var enemyFactory = new EnemyFactory(partyManager);
            enemyManager = new EnemyManager(enemyFactory);

            playerAI.Init(enemyManager, playableCharacterActionManager);
        }

        public void Start()
        {
            ObserveEncountEnemy();
        }
        
        private void ObserveEncountEnemy()
        {
            fieldManager.EncountedEnemyObservable
                .Subscribe(enemyType =>
                {
                    GenerateEnemies(enemyType);
                    turnManager.StartTurn();
                    SelectPlayableCharactersAction();
                    StartBattle();
                })
                .AddTo(this);
        }

        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: �l�X�Ȑ����̃p�^�[������������i������̐����⌈�߂�ꂽ�p�^�[���̐����j
            //NOTE: ���݂͈�̂�����������
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        private void SelectPlayableCharactersAction()
        {
            playableCharacterActionManager.ClearDic();
            //�v���C���[�ɃL�����N�^�[�̍s�������߂�����
            playerAI.SelectCharacterAction();
        }

        private void StartBattle()
        {
            ProcessBattle().Forget();
        }

        private async UniTaskVoid ProcessBattle()
        {
            while (true)
            {
                var characters = GetSortedCharacterByAgility();
                foreach(var character in characters)
                {
                    //���ꂼ��̃L�����̍s�������s����
                    ExecuteCharacterAction(character);

                    var result = CheckBattleResult();
                    if (result != ResultType.None)
                    {
                        FinishBattle(result);
                        return;
                    }

                    await UniTask.DelayFrame(1);
                }

                Debug.Log("�^�[���I��");
                turnManager.NextTurn();
                SelectPlayableCharactersAction();
            }
        }

        private void ExecuteCharacterAction(BaseCharacter character)
        {
            if(character is PlayableCharacter)
            {
                var action = playableCharacterActionManager.GetCharacterAction(character as PlayableCharacter);
                var result = action.ExecuteAction();
                if (!result)
                {
                    switch (action.targetType)
                    {
                        case TargetType.Self:
                            break;
                        case TargetType.Friends:
                            action.action.Invoke(character);
                            break;
                        case TargetType.Enemy:
                            var target = enemyManager.enemies.RandomGet();
                            action.action.Invoke(target);
                            break;
                    }
                }
            }
            else if(character is EnemyCharacter)
            {
                var action = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var result = action.ExecuteAction();
                if (!result)
                {
                    switch (action.targetType)
                    {
                        case TargetType.Self:
                            break;
                        case TargetType.Friends:
                            action.action.Invoke(character);
                            break;
                        case TargetType.Enemy:
                            var target = partyManager.partyCharacters.RandomGet();
                            action.action.Invoke(target);
                            break;
                    }
                }
            }
        }

        private ResultType CheckBattleResult()
        {
            if (enemyManager.enemies.All(x => x.IsDead))
                return ResultType.Win;

            if (partyManager.partyCharacters.All(x => x.IsDead))
                return ResultType.Lose;

            return ResultType.None;
        }

        private void FinishBattle(ResultType result)
        {
            Debug.Log("Finish");
        }

        private IEnumerable<BaseCharacter> GetSortedCharacterByAgility()
            => enemyManager.enemies.Cast<BaseCharacter>()
                .Concat(partyManager.partyCharacters)
                .OrderBy(x => x.characterStatus.Agility);
    }
}

