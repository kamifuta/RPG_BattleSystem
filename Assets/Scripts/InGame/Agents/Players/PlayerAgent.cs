using InGame.Buttles;
using InGame.Buttles.Actions;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Items;
using InGame.Magics;
using InGame.Parties;
using InGame.Skills;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VContainer;
using UniRx;
using Unity.MLAgents.Policies;

namespace InGame.Agents.Players
{
    public class PlayerAgent : Agent
    {
        public PlayableCharacter agentCharacter { get; private set; }

        protected PartyManager partyManager;
        protected EnemyManager enemyManager;

        public Action OnEpisodeBeginEvent;
        public bool HadSelectedAction { get; private set; } = false;

        private const int BaseAction = 0;
        private const int ItemAction = 1;
        private const int SkillAction = 2;
        private const int MagicAction = 3;
        private const int LivingEnemyAction = 4;
        private const int LivingPlayerAction = 5;
        private const int DeadPlayerAction = 6;

        private int addDamage = 0;
        private CompositeDisposable disposable = new CompositeDisposable();

        private readonly Subject<ActionData> selectedActionDataSubject = new Subject<ActionData>();
        public IObservable<ActionData> SelectedActionDataObservable => selectedActionDataSubject;

        public void Init(PartyManager partyManager, EnemyManager enemyManager)
        {
            this.partyManager = partyManager;
            this.enemyManager = enemyManager;
            addDamage = 0;

            disposable?.Dispose();
            ObserveAddDamageForEnemy();
        }

        public void SetAgentCharacter(PlayableCharacter agentCharacter)
        {
            this.agentCharacter = agentCharacter;
        }

        private void ObserveAddDamageForEnemy()
        {
            foreach(var enemyHealth in enemyManager.enemies.Select(x=>x.characterHealth))
            {
                var disposable1 = enemyHealth.DamagedValueObservable
                    .Subscribe(x => addDamage += x)
                    .AddTo(this);

                var disposable2 = enemyHealth.HealValueObservable
                    .Subscribe(x => addDamage -= x)
                    .AddTo(this);

                disposable.Add(disposable1);
                disposable.Add(disposable2);
            }
        }

        public override void OnEpisodeBegin()
        {
            OnEpisodeBeginEvent?.Invoke();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(agentCharacter.HPRate);
            sensor.AddObservation(agentCharacter.MPRate);

            var partyCharacters = partyManager.partyCharacters.Where(x => x != agentCharacter).ToList();

            sensor.AddObservation(partyCharacters.Select(x => x.HPRate).ToList());
            sensor.AddObservation(partyCharacters.Select(x => x.MPRate).ToList());

            var characterStatus = agentCharacter.characterStatus;

            sensor.AddObservation(characterStatus.AttackValue);
            sensor.AddObservation(characterStatus.MagicValue);
            sensor.AddObservation(characterStatus.DefenceValue);
            sensor.AddObservation(characterStatus.MagicDefenceValue);
            sensor.AddObservation(characterStatus.Agility);

            var partyStatus = partyCharacters.Select(x => x.characterStatus).ToList();

            sensor.AddObservation(partyStatus.Select(x=>(float)x.AttackValue).ToList());
            sensor.AddObservation(partyStatus.Select(x=>(float)x.MagicValue).ToList());
            sensor.AddObservation(partyStatus.Select(x=>(float)x.DefenceValue).ToList());
            sensor.AddObservation(partyStatus.Select(x=>(float)x.MagicDefenceValue).ToList());
            sensor.AddObservation(partyStatus.Select(x=>(float)x.Agility).ToList());

            var characterBuff = characterStatus.characterBuff;

            sensor.AddObservation(characterBuff.AttackBuffLevel);
            sensor.AddObservation(characterBuff.MagicBuffLevel);
            sensor.AddObservation(characterBuff.DefenceBuffLevel);
            sensor.AddObservation(characterBuff.MagicDefenceBuffLevel);
            sensor.AddObservation(characterBuff.AgilityBuffLevel);

            var partyBuffs = partyStatus.Select(x => x.characterBuff).ToList();

            sensor.AddObservation(partyBuffs.Select(x=>(float)x.AttackBuffLevel).ToList());
            sensor.AddObservation(partyBuffs.Select(x=>(float)x.MagicBuffLevel).ToList());
            sensor.AddObservation(partyBuffs.Select(x=>(float)x.DefenceBuffLevel).ToList());
            sensor.AddObservation(partyBuffs.Select(x=>(float)x.MagicDefenceBuffLevel).ToList());
            sensor.AddObservation(partyBuffs.Select(x=>(float)x.AgilityBuffLevel).ToList());

            var enemyBuffs = enemyManager.enemies.Select(x => x.characterStatus.characterBuff).ToList();

            sensor.AddObservation(enemyBuffs.Select(x => (float)x.AttackBuffLevel).ToList());
            sensor.AddObservation(enemyBuffs.Select(x => (float)x.MagicBuffLevel).ToList());
            sensor.AddObservation(enemyBuffs.Select(x => (float)x.DefenceBuffLevel).ToList());
            sensor.AddObservation(enemyBuffs.Select(x => (float)x.MagicDefenceBuffLevel).ToList());
            sensor.AddObservation(enemyBuffs.Select(x => (float)x.AgilityBuffLevel).ToList());

            sensor.AddObservation(addDamage);
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            var itemEnumValues = Enum.GetValues(typeof(ItemType));
            var skillEnumValues = Enum.GetValues(typeof(SkillType));
            var magicEnumValues = Enum.GetValues(typeof(MagicType));

            var character = agentCharacter;

            if (character.HaveItemList.Count <= 0)
            {
                actionMask.SetActionEnabled(BaseAction, (int)BaseActionType.UseItem, false);
            }
            else
            {
                foreach (var item in itemEnumValues)
                {
                    actionMask.SetActionEnabled(ItemAction, (int)item, character.HaveItemList.Any(x => x == (ItemType)item));
                }
            }

            if (character.rememberSkills.Count > 0 && character.rememberSkills.Select(x => SkillDataBase.GetSkillData(x)).Any(x => x.consumeMP < character.characterMagic.currentMP))
            {
                foreach (var skill in skillEnumValues)
                {
                    var enable = character.rememberSkills.Any(x => x == (SkillType)skill) && SkillDataBase.GetSkillData((SkillType)skill).consumeMP < character.characterMagic.currentMP;
                    actionMask.SetActionEnabled(SkillAction, (int)skill, enable);
                }
            }
            else
            {
                actionMask.SetActionEnabled(BaseAction, (int)BaseActionType.UseSkill, false);
            }

            if (character.rememberMagics.Count > 0 && character.rememberMagics.Select(x => MagicDataBase.GetMagicData(x)).Any(x => x.consumeMP < character.characterMagic.currentMP))
            {
                foreach (var magic in magicEnumValues)
                {
                    var magicData = MagicDataBase.GetMagicData((MagicType)magic);

                    if (magicData.IsTargetableDeadCharacter && partyManager.partyCharacters.All(x=>x.characterHealth.IsDead))
                    {
                        actionMask.SetActionEnabled(MagicAction, (int)magic, false);
                        continue;
                    }

                    var enable = character.rememberMagics.Any(x => x == (MagicType)magic) && magicData.consumeMP < character.characterMagic.currentMP;
                    actionMask.SetActionEnabled(MagicAction, (int)magic, enable);
                }
            }
            else
            {
                actionMask.SetActionEnabled(BaseAction, (int)BaseActionType.UseMagic, false);
            }

            for (int i = 0; i < enemyManager.enemies.Length; i++)
            {
                var enemy = enemyManager.enemies[i];
                actionMask.SetActionEnabled(LivingEnemyAction, i, !enemy.characterHealth.IsDead);
            }

            if (partyManager.partyCharacters.Any(x => x.characterHealth.IsDead))
            {
                for (int i = 0; i < partyManager.partyCharacters.Length; i++)
                {
                    var player = partyManager.partyCharacters[i];
                    actionMask.SetActionEnabled(LivingPlayerAction, i, !player.characterHealth.IsDead);
                    actionMask.SetActionEnabled(DeadPlayerAction, i, player.characterHealth.IsDead);
                }
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            BaseCharacter target;

            var character = agentCharacter;

            if (character.characterHealth.IsDead)
            {
                HadSelectedAction = true;
                return;
            }

            var actionType = actionBuffers.DiscreteActions[0];
            int targetIndex;
            ActionData action = null;

            switch (actionType)
            {
                case (int)BaseActionType.NormalAttack:
                    targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                    target = enemyManager.enemies[targetIndex];
                    action = new ActionData(BaseActionType.NormalAttack, character, target);
                    break;
                case (int)BaseActionType.Defence:
                    action = new ActionData(BaseActionType.Defence, character);
                    break;
                case (int)BaseActionType.UseItem:
                    var itemType = (ItemType)Enum.ToObject(typeof(ItemType), actionBuffers.DiscreteActions[ItemAction]);
                    var itemData = ItemDataBase.GetItemData(itemType);
                    switch (itemData.targetType)
                    {
                        case TargetType.Self:
                            action = new ActionData(BaseActionType.UseItem, character, character, itemType);
                            break;
                        case TargetType.Friends:
                            targetIndex = actionBuffers.DiscreteActions[LivingPlayerAction];
                            target = partyManager.partyCharacters[targetIndex];
                            action = new ActionData(BaseActionType.UseItem, character, target, itemType);
                            break;
                        case TargetType.Enemy:
                            targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                            target = enemyManager.enemies[targetIndex];
                            action = new ActionData(BaseActionType.UseItem, character, target, itemType);
                            break;
                        case TargetType.AllFriends:
                            break;
                        case TargetType.AllEnemy:
                            break;
                    }
                    break;
                case (int)BaseActionType.UseSkill:
                    var skillType = (SkillType)Enum.ToObject(typeof(ItemType), actionBuffers.DiscreteActions[SkillAction]);
                    var skillData = SkillDataBase.GetSkillData(skillType);
                    switch (skillData.targetType)
                    {
                        case TargetType.Self:
                            action = new ActionData(BaseActionType.UseSkill, character, character, skillType);
                            break;
                        case TargetType.Friends:
                            targetIndex = actionBuffers.DiscreteActions[LivingPlayerAction];
                            target = partyManager.partyCharacters[targetIndex];
                            action = new ActionData(BaseActionType.UseSkill, character, target, skillType);
                            break;
                        case TargetType.Enemy:
                            targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                            target = enemyManager.enemies[targetIndex];
                            action = new ActionData(BaseActionType.UseSkill, character, target, skillType);
                            break;
                        case TargetType.AllFriends:
                            break;
                        case TargetType.AllEnemy:
                            action = new ActionData(BaseActionType.UseSkill, character, enemyManager.enemies, skillType);
                            break;
                    }
                    break;
                case (int)BaseActionType.UseMagic:
                    var magicType = (MagicType)Enum.ToObject(typeof(MagicType), actionBuffers.DiscreteActions[MagicAction]);
                    var magicData = MagicDataBase.GetMagicData(magicType);
                    switch (magicData.targetType)
                    {
                        case TargetType.Self:
                            action = new ActionData(BaseActionType.UseMagic, character, character, magicType);
                            break;
                        case TargetType.Friends:
                            targetIndex = magicData.IsTargetableDeadCharacter ? actionBuffers.DiscreteActions[DeadPlayerAction] : actionBuffers.DiscreteActions[LivingPlayerAction];
                            target = partyManager.partyCharacters[targetIndex];
                            action = new ActionData(BaseActionType.UseMagic, character, target, magicType);
                            break;
                        case TargetType.Enemy:
                            targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                            target = enemyManager.enemies[targetIndex];
                            action = new ActionData(BaseActionType.UseMagic, character, target, magicType);
                            break;
                        case TargetType.AllFriends:
                            break;
                        case TargetType.AllEnemy:
                            break;
                    }
                    break;
            }

            selectedActionDataSubject.OnNext(action);
            HadSelectedAction = true;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {

        }

        public void ClearFlag()
        {
            HadSelectedAction = false;
        }
    }
}

