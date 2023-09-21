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

namespace InGame.Agents.Players
{
    public class PlayerAgent : Agent
    {
        public PlayableCharacter agentCharacter { get; private set; }

        protected PartyManager partyManager;
        protected EnemyManager enemyManager;
        protected PlayableCharacterActionManager playableCharacterActionManager;

        public Action OnEpisodeBeginEvent;
        public bool HadSelectedAction { get; private set; } = false;

        private const int BaseAction = 0;
        private const int ItemAction = 1;
        private const int SkillAction = 2;
        private const int MagicAction = 3;
        private const int LivingEnemyAction = 4;
        private const int PlayerAction = 5;

        private int addDamage = 0;
        private IDisposable disposable;

        public void Init(PartyManager partyManager, EnemyManager enemyManager, PlayableCharacterActionManager playableCharacterActionManager)
        {
            this.partyManager = partyManager;
            this.enemyManager = enemyManager;
            this.playableCharacterActionManager = playableCharacterActionManager;
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
            foreach(var enemy in enemyManager.enemies)
            {
                disposable= enemy.AttackerObservable
                    .Where(t=>t.Item1==agentCharacter)
                    .Subscribe(t =>
                    {
                        addDamage += t.Item2;
                    })
                    .AddTo(this);
            }
        }

        public override void OnEpisodeBegin()
        {
            OnEpisodeBeginEvent?.Invoke();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(partyManager.partyCharacters.Select(x => x.HPRate).ToList());
            sensor.AddObservation(partyManager.partyCharacters.Select(x => x.MPRate).ToList());
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
                    var enable = character.rememberMagics.Any(x => x == (MagicType)magic) && MagicDataBase.GetMagicData((MagicType)magic).consumeMP < character.characterMagic.currentMP;
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
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                    break;
                case (int)BaseActionType.Defence:
                    action = new ActionData(BaseActionType.Defence, character);
                    playableCharacterActionManager.SetPlayableCharacterAction(character, action);
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
                            targetIndex = actionBuffers.DiscreteActions[PlayerAction];
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
                    playableCharacterActionManager.SetPlayableCharacterAction(agentCharacter, action);
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
                            targetIndex = actionBuffers.DiscreteActions[PlayerAction];
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
                            break;
                    }
                    playableCharacterActionManager.SetPlayableCharacterAction(agentCharacter, action);
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
                            targetIndex = actionBuffers.DiscreteActions[PlayerAction];
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
                    playableCharacterActionManager.SetPlayableCharacterAction(agentCharacter, action);
                    break;
            }

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

