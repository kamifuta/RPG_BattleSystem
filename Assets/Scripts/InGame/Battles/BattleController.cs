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
using InGame.Skills;
using InGame.Buttles.Actions;
using InGame.Items;
using System.Threading;
using Unity.MLAgents;
using InGame.Agents.Players;
using InGame.Agents;

namespace InGame.Buttles
{
    public class BattleController : ControllerBase, IStartable, IDisposable
    {
        public enum ResultType
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
        //private PlayerAI playerAI;
        //private PlayerAgent playerAgent;
        private SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();
        //private RewardProvider rewardProvider;

        private List<BaseCharacter> hadDoneActionCharacterList= new List<BaseCharacter>();
        private CancellationTokenSource cancellationTokenSource;

        private int battleCount = 0;
        private int winCount = 0;

        private bool IsBattle = false;

        private ISubject<ResultType> resultSubject = new Subject<ResultType>();
        public IObservable<ResultType> ResultObservable => resultSubject;

        [Inject]
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAgent[] playerAgents, EnemyFactory enemyFactory/*, RewardProvider rewardProvider*/)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            //this.playerAI = playerAI;
            //this.playerAgent = playerAgent;
            foreach(var agent in playerAgents)
            {
                agentGroup.RegisterAgent(agent);
            }
            //this.rewardProvider = rewardProvider;

            //enemyFactory = new EnemyFactory(partyManager);
            this.enemyFactory = enemyFactory;
        }

        public void Start()
        {
            ObserveEncountEnemy();
            //playerAgent.OnEpisodeBeginEvent += Encount;
        }

        private void ObserveEncountEnemy()
        {
            fieldManager.EncountedEnemyObservable
                .Subscribe(enemyType =>
                {
                    //playerAgent.gameObject.SetActive(true);

                    //LogWriter.SetFileName();

                    ////他クラスの初期化
                    //enemyManager = new EnemyManager(enemyFactory);
                    ////playerAI.Init(enemyManager, playableCharacterActionManager);
                    //playerAgent.Init(partyManager, enemyManager, playableCharacterActionManager);

                    ////フィールドの生成
                    //GenerateEnemies(enemyType);
                    //LogCharacterStatus();
                    //turnManager.StartTurn();

                    //StartBattle();
                })
                .AddTo(this);
        }

        public void Encount()
        {
            LogWriter.SetFileName();
            IsBattle = true;

            //他クラスの初期化
            enemyManager = new EnemyManager(enemyFactory);
            //playerAI.Init(enemyManager, playableCharacterActionManager);
            //playerAgent.Init(partyManager, enemyManager, playableCharacterActionManager);
            int i = 0;
            foreach(var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).Init(partyManager.partyCharacters[i], partyManager, enemyManager, playableCharacterActionManager);
                i++;
            }
            resultSubject = new Subject<ResultType>();

            //フィールドの生成
            GenerateEnemies(EnemyType.Golem);
            LogCharacterStatus();
            turnManager.StartTurn();

            //rewardProvider.AddRewardByAttack(enemyManager);

            StartBattle();
        }

        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: 様々な生成のパターンを実装する（複数種の生成や決められたパターンの生成）
            //NOTE: 現在は一種類だけ生成する
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        private async UniTask SelectPlayableCharactersAction(CancellationToken token)
        {
            playableCharacterActionManager.ClearDic();
            //プレイヤーにキャラクターの行動を決めさせる
            //playerAI.SelectCharacterAction();
            foreach(var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).RequestDecision();
                await UniTask.DelayFrame(1, cancellationToken: token);
            }
            //playerAgent.RequestDecision();
        }

        private void StartBattle()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            ProcessBattle(cancellationTokenSource.Token).Forget();
        }

        private async UniTask ProcessBattle(CancellationToken token)
        {
            while (true)
            {
                //playerAgent.AddReward(-0.01f);
                hadDoneActionCharacterList.Clear();

                LogCharacterHPAndMP();

                try
                {
                    await SelectPlayableCharactersAction(token);
                }
                catch(InvalidOperationException)
                {
                    break;
                }
                
                await UniTask.WaitUntil(() => agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().All(x=>x.HadSelectedAction));

                try
                {
                    //優先度の高い行動を行う
                    ExecuteDefenceAction();
                    ExecuteHighPriorityAction();

                    ExecuteNormalPriorityAction();

                    ExecuteLowPriorityAction();
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await UniTask.DelayFrame(1, cancellationToken: token);
                //rewardProvider.AddRewardByDefence();
                
                ClearCharacterBuff();
                turnManager.NextTurn();
                foreach (var agent in agentGroup.GetRegisteredAgents())
                {
                    (agent as PlayerAgent).ClearFlag();
                }
                //playerAgent.ClearFlag();

                if (turnManager.turnCount > 100)
                {
                    FinishBattle(ResultType.Lose);
                    break;
                }
            }
        }

        private void ExecuteDefenceAction()
        {
            var actionPairs = playableCharacterActionManager.GetDefenceActionPairs().OrderByDescending(x=>x.Key.characterStatus.Agility);
            foreach (var actionPair in actionPairs)
            {
                actionPair.Value.ExecuteAction();
                hadDoneActionCharacterList.Add(actionPair.Key);

                if (!IsBattle)
                    return;
                //await UniTask.DelayFrame(1, cancellationToken:token);
            }
        }

        private void ExecuteHighPriorityAction()
        {
            var characters = playableCharacterActionManager.GetHighPriorityActionCharacters().OrderByDescending(x=>x.characterStatus.Agility);
            foreach(var character in characters)
            {
                ExecuteCharacterAction(character);

                if (!IsBattle)
                    return;
                //await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteNormalPriorityAction()
        {
            var players = playableCharacterActionManager.GetNormalPriorityActionCharacters();
            var sortedCharacters= enemyManager.enemies.Where(x => !x.characterHealth.IsDead)
                                                .Cast<BaseCharacter>()
                                                .Concat(players)
                                                .OrderByDescending(x => x.characterStatus.Agility);

            foreach(var character in sortedCharacters)
            {
                //Debug.Log(character.characterName);
                ExecuteCharacterAction(character);

                if (!IsBattle)
                    return;
                //await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteLowPriorityAction()
        {
            var characters = playableCharacterActionManager.GetLowPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);
            foreach (var character in characters)
            {
                ExecuteCharacterAction(character);

                if (!IsBattle)
                    return;
                //await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteCharacterAction(BaseCharacter character)
        {
            if (character.characterHealth.IsDead)
                return;

            if(character is PlayableCharacter)
            {
                var actionData = playableCharacterActionManager.GetCharacterActionData(character as PlayableCharacter);
                var result = actionData.ExecuteAction();
                if (!result)
                {
                    BaseCharacter target = Retarget(actionData, enemyManager.enemies);
                    if (target == null)
                    {
                        actionData.ExecuteAction(character);
                    }
                    else
                    {
                        actionData.ExecuteAction(target);
                    }
                }
            }
            else if(character is EnemyCharacter)
            {
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var result = actionData.ExecuteAction();
                if (!result)
                {
                    BaseCharacter target = Retarget(actionData, partyManager.partyCharacters);
                    if (target == null)
                    {
                        actionData.ExecuteAction(character);
                    }
                    else
                    {
                        actionData.ExecuteAction(target);
                    }
                }
            }
            hadDoneActionCharacterList.Add(character);

            CheckBattleResult();
        }

        private BaseCharacter Retarget(ActionData actionData, IEnumerable<BaseCharacter> targetGroup)
        {
            switch (actionData.actionType)
            {
                case BaseActionType.NormalAttack:
                    return targetGroup.Where(x => !x.characterHealth.IsDead).RandomGet();
                case BaseActionType.UseItem:
                    var item = ItemDataBase.GetItemData(actionData.itemType);
                    switch (item.targetType)
                    {
                        case TargetType.Friends:
                            return null;
                        case TargetType.Enemy:
                            return targetGroup.Where(x => !x.characterHealth.IsDead).RandomGet();
                    }
                    break;
                case BaseActionType.UseSkill:
                    break;
            }
            return null;
        }

        private void CheckBattleResult()
        {
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Win);
                return;
            }

            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Lose);
                return;
            }
        }

        private void ClearCharacterBuff()
        {
            foreach(var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        private void FinishBattle(ResultType result)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            battleCount++;
            IsBattle = false;

            switch (result)
            {
                case ResultType.Win:
                    Debug.Log("勝利");
                    LogWriter.WriteLog($"\n勝利");
                    agentGroup.SetGroupReward(1f);
                    //playerAgent.SetReward(1f);
                    winCount++;
                    break;
                case ResultType.Lose:
                    Debug.Log("敗北");
                    LogWriter.WriteLog($"\n負け");
                    agentGroup.SetGroupReward(-1f);
                    //playerAgent.SetReward(-1f);
                    break;
            }

            enemyManager.Dispose();
            
            Debug.Log("Finish Battle");
            Debug.Log("勝率：" + (float)winCount / battleCount);
            LogCharacterStatus();

            //partyManager.InitParty();

            //playerAgent.EndEpisode();
            agentGroup.EndGroupEpisode();

            resultSubject.OnNext(result);
            resultSubject.OnCompleted();

            //Encount();
        }

        private IEnumerable<BaseCharacter> AllCharacters
            => enemyManager.enemies.Cast<BaseCharacter>().Concat(partyManager.partyCharacters);

        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"味方のステータス--------------------");
            foreach(var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP} " +
                    $"攻撃力{character.characterStatus.AttackValue} 魔力{character.characterStatus.MagicValue} 防御力{character.characterStatus.DefecnceValue} 魔法防御力{character.characterStatus.MagicDefecnceValue} 素早さ{character.characterStatus.Agility}" +
                    $"スキル({character.rememberSkills.Enumerate()}) 魔法({character.rememberMagics.Enumerate()})");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"敵のステータス--------------------");
            foreach(var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP} MP{enemy.characterMagic.currentMP}/{enemy.characterStatus.MaxMP} " +
                    $"攻撃力{enemy.characterStatus.AttackValue} 魔力{enemy.characterStatus.MagicValue} 防御力{enemy.characterStatus.DefecnceValue} 魔法防御力{enemy.characterStatus.MagicDefecnceValue} 素早さ{enemy.characterStatus.Agility}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        private void LogCharacterHPAndMP()
        {
            LogWriter.WriteLog($"味方のステータス--------------------");
            foreach (var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP}" +
                    $"所持アイテム({character.HaveItemList.Enumerate()})");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"敵のステータス--------------------");
            foreach (var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP} MP{enemy.characterMagic.currentMP}/{enemy.characterStatus.MaxMP}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            enemyManager?.Dispose();
        }
    }
}

