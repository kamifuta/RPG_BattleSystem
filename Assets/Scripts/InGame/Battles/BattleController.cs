using InGame.Characters;
using InGame.Fields;
using InGame.Parties;
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
using Cysharp.Threading.Tasks;
using Log;
using System;
using InGame.Characters.Skills;
using InGame.Buttles.Actions;
using InGame.Items;

namespace InGame.Buttles
{
    public class BattleController : ControllerBase, IStartable, IDisposable
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

        private EnemyFactory enemyFactory;
        private PartyManager partyManager;
        private FieldManager fieldManager;
        private PlayerAI playerAI;

        [Inject]
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAI playerAI)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            this.playerAI = playerAI;

            enemyFactory = new EnemyFactory(partyManager);
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
                    LogWriter.SetFileName();

                    //���N���X�̏�����
                    enemyManager = new EnemyManager(enemyFactory);
                    playerAI.Init(enemyManager, playableCharacterActionManager);

                    //�t�B�[���h�̐���
                    GenerateEnemies(enemyType);
                    turnManager.StartTurn();
                    
                    StartBattle();
                })
                .AddTo(this);
        }

        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: �l�X�Ȑ����̃p�^�[������������i������̐����⌈�߂�ꂽ�p�^�[���̐����j
            //NOTE: ���݂͈��ނ�����������
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
                LogCharacterStatus();

                SelectPlayableCharactersAction();
                ExecuteHighPriorityAction();

                var characters = GetSortedCharacterByAgility();
                foreach(var character in characters)
                {
                    if (character.characterHealth.IsDead)
                        continue;

                    if (character.HadDoneAction)
                        continue;

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

                ResetFlag();
                ClearCharacterBuff();
                turnManager.NextTurn();
            }
        }

        private void ExecuteHighPriorityAction()
        {
            var actions = playableCharacterActionManager.GetDefenceActionPairs();
            foreach(var action in actions)
            {
                action.Value.ExecuteAction();
                action.Key.SetHadDoneAction(true);
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
                    BaseCharacter target;

                    switch (action.actionType)
                    {
                        case BaseActionType.NormalAttack:
                            target = enemyManager.enemies.Where(x => !x.characterHealth.IsDead).RandomGet();
                            action.ExecuteAction(target);
                            break;
                        case BaseActionType.UseItem:
                            var item = ItemDataBase.GetItemData(action.itemType);
                            switch (item.targetType)
                            {
                                case TargetType.Friends:
                                    action.ExecuteAction(character);
                                    break;
                                case TargetType.Enemy:
                                    target = enemyManager.enemies.Where(x => !x.characterHealth.IsDead).RandomGet();
                                    action.ExecuteAction(target);
                                    break;
                            }
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
                    BaseCharacter target;

                    switch (action.actionType)
                    {
                        case BaseActionType.NormalAttack:
                            target = partyManager.partyCharacters.Where(x => !x.characterHealth.IsDead).RandomGet();
                            action.ExecuteAction(target);
                            break;
                        case BaseActionType.UseItem:
                            var item = ItemDataBase.GetItemData(action.itemType);
                            switch (item.targetType)
                            {
                                case TargetType.Friends:
                                    action.ExecuteAction(character);
                                    break;
                                case TargetType.Enemy:
                                    target = partyManager.partyCharacters.Where(x => !x.characterHealth.IsDead).RandomGet();
                                    action.ExecuteAction(target);
                                    break;
                            }
                            break;
                    }
                }
            }
            character.SetHadDoneAction(true);
        }

        private ResultType CheckBattleResult()
        {
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
                return ResultType.Win;

            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
                return ResultType.Lose;

            return ResultType.None;
        }

        private void ClearCharacterBuff()
        {
            foreach(var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        private void ResetFlag()
        {
            foreach (var character in AllCharacters)
            {
                character.ResetFlag();
            }
        }

        private void FinishBattle(ResultType result)
        {
            switch (result)
            {
                case ResultType.Win:
                    LogWriter.WriteLog($"����");
                    break;
                case ResultType.Lose:
                    LogWriter.WriteLog($"����");
                    break;
            }

            enemyManager.Dispose();

            Debug.Log("Finish Battle");
            LogCharacterStatus();
        }

        private IEnumerable<BaseCharacter> GetSortedCharacterByAgility()
            => enemyManager.enemies.Where(x=>!x.characterHealth.IsDead)
                .Cast<BaseCharacter>()
                .Concat(partyManager.partyCharacters)
                .OrderByDescending(x => x.characterStatus.Agility);

        private IEnumerable<BaseCharacter> AllCharacters
            => enemyManager.enemies.Cast<BaseCharacter>().Concat(partyManager.partyCharacters);

        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------");
            foreach(var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP}");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------");
            foreach(var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        public void Dispose()
        {
            enemyManager.Dispose();
        }
    }
}

