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
    public class BattleController : ControllerBase,　IDisposable
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
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAgent[] playerAgents, EnemyFactory enemyFactory)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            foreach(var agent in playerAgents)
            {
                agentGroup.RegisterAgent(agent);
            }

            this.enemyFactory = enemyFactory;
        }

        /// <summary>
        /// 敵と接触した時に呼び出される
        /// </summary>
        public void Encount()
        {
            LogWriter.SetFileName();
            IsBattle = true;

            //他クラスの初期化
            enemyManager = new EnemyManager(enemyFactory);
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

            //戦闘を開始する
            StartBattle();
        }

        /// <summary>
        /// 敵を生成する
        /// </summary>
        /// <param name="encountedEnemyType"></param>
        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: 様々な生成のパターンを実装する（複数種の生成や決められたパターンの生成）
            //NOTE: 現在は一種類だけ生成する
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        /// <summary>
        /// 戦闘を開始する
        /// </summary>
        private void StartBattle()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            ProcessBattle(cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 戦闘を進める
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ProcessBattle(CancellationToken token)
        {
            //戦闘が終了するまでループする
            while (IsBattle)
            {
                //ターン開始時の初期化
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
                
                //すべてのエージェントが行動を決定するまで待機
                await UniTask.WaitUntil(() => agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().All(x=>x.HadSelectedAction));

                ExecuteActions();
                if (!IsBattle)
                    break;

                await UniTask.DelayFrame(1, cancellationToken: token);
                
                ClearCharacterBuff();
                turnManager.NextTurn();
                foreach (var agent in agentGroup.GetRegisteredAgents())
                {
                    (agent as PlayerAgent).ClearFlag();
                }
            }
        }

        //プレイヤーに行動を決定させる
        private async UniTask SelectPlayableCharactersAction(CancellationToken token)
        {
            playableCharacterActionManager.ClearDic();
            //各エージェントに行動を決定させる
            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).RequestDecision();
                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteActions()
        {
            //防御するキャラクター
            IEnumerable<BaseCharacter> defenceActionCharacters = playableCharacterActionManager.GetDefenceActionPairs().OrderByDescending(x => x.characterStatus.Agility);
            //優先度の高い行動をするキャラクター
            IEnumerable<BaseCharacter> highPriorityActionCharacters= playableCharacterActionManager.GetHighPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);
            //通常優先度の行動をするプレイヤーキャラクター
            IEnumerable<BaseCharacter> normalPriorityActionPlayers = playableCharacterActionManager.GetNormalPriorityActionCharacters();
            //通常優先度の行動をするキャラクター
            IEnumerable<BaseCharacter> normalPriorityActionCharacters= enemyManager.enemies.Where(x => !x.characterHealth.IsDead)
                                                .Cast<BaseCharacter>()
                                                .Concat(normalPriorityActionPlayers)
                                                .OrderByDescending(x => x.characterStatus.Agility);
            //優先度の低い行動をするキャラクター
            IEnumerable<BaseCharacter> lowPriorityActionCharacters= playableCharacterActionManager.GetLowPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);

            //行動順にソートされたキャラクター
            IEnumerable<BaseCharacter> sortedCharacters = defenceActionCharacters.Concat(highPriorityActionCharacters).Concat(normalPriorityActionCharacters).Concat(lowPriorityActionCharacters);
            foreach(var character in sortedCharacters)
            {
                ExecuteCharacterAction(character);

                if (!IsBattle)
                    return;
            }
        }

        /// <summary>
        /// キャラクターの行動を実行する
        /// </summary>
        /// <param name="character"></param>
        private void ExecuteCharacterAction(BaseCharacter character)
        {
            //キャラクターが死んでいるなら行動しない
            if (character.characterHealth.IsDead)
                return;

            //キャラクターがプレイヤーのキャラクターだったとき
            if(character is PlayableCharacter)
            {
                var actionData = playableCharacterActionManager.GetCharacterActionData(character as PlayableCharacter);
                var success = actionData.ExecuteAction();
                //アクションが成功しなかったとき（ターゲットがすでに死んでいるなど）
                if (!success)
                {
                    //生きている敵キャラからターゲットを選びなおす
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
            //キャラクターが敵キャラだったとき
            else if(character is EnemyCharacter)
            {
                //敵AIに行動を決めさせる
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var success = actionData.ExecuteAction();
                //アクションが成功しなかったとき（ターゲットがすでに死んでいるなど）
                if (!success)
                {
                    //生きているプレイヤーキャラクターからターゲットを選びなおす
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

            //行動終了しているキャラクターのリストに追加する
            hadDoneActionCharacterList.Add(character);

            //戦闘が終了しているかを確認する
            CheckBattleResult();
        }

        /// <summary>
        /// ターゲットを変更する
        /// </summary>
        /// <param name="actionData"></param>
        /// <param name="targetGroup"></param>
        /// <returns>
        /// 新しいターゲットを返す
        /// 新しいターゲットが行動者と一致するならnullを返す
        /// </returns>
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

        /// <summary>
        /// 戦闘が終了しているかを調べる
        /// </summary>
        private void CheckBattleResult()
        {
            //敵が全員死んでいるときプレイヤーの勝ち
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Win);
                return;
            }

            //見方が全員死んでいるときプレイヤーの負け
            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Lose);
                return;
            }
        }

        /// <summary>
        /// キャラクターにかかっているバフを消す
        /// </summary>
        private void ClearCharacterBuff()
        {
            foreach (var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        /// <summary>
        /// 戦闘終了時の処理
        /// </summary>
        /// <param name="result"></param>
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
                    winCount++;
                    break;
                case ResultType.Lose:
                    Debug.Log("敗北");
                    LogWriter.WriteLog($"\n負け");
                    agentGroup.SetGroupReward(-1f);
                    break;
            }

            enemyManager.Dispose();
            
            Debug.Log("Finish Battle");
            Debug.Log("勝率：" + (float)winCount / battleCount);
            LogCharacterStatus();

            agentGroup.EndGroupEpisode();

            resultSubject.OnNext(result);
            resultSubject.OnCompleted();
        }

        /// <summary>
        /// 敵味方すべてのキャラクターのリストを返す
        /// </summary>
        private IEnumerable<BaseCharacter> AllCharacters
            => enemyManager.enemies.Cast<BaseCharacter>().Concat(partyManager.partyCharacters);

        /// <summary>
        /// 敵味方のステータスをすべて書き込む
        /// </summary>
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

        /// <summary>
        /// 味方のHP,MPとアイテム
        /// 敵のHP,MPを表示する
        /// </summary>
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

