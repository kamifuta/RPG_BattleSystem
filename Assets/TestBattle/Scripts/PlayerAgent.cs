using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace TestButtle
{
    public enum ActionType
    {
        None,
        Attack,
        Heal,
    }

    public class PlayerAgent : Agent
    {
        [SerializeField] private ButtleController buttleController;
        [SerializeField] private ButtleManager buttleManager;

        private Character controllPlayer;

        private void FixedUpdate()
        {
            if (controllPlayer == null)
                return;

            if (buttleController.currentActionCharacter == controllPlayer && !controllPlayer.HadDoneAction)
            {
                RequestDecision();
            }
        }

        public override void OnEpisodeBegin()
        {
            buttleController.StartButtle();
            controllPlayer = buttleManager.player;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(controllPlayer.currentHP);
            sensor.AddObservation(controllPlayer.currentMP);
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(0, (int)ActionType.None, false);
            if (controllPlayer.currentMP <= 0)
            {
                actionMask.SetActionEnabled(0, (int)ActionType.Heal, false);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var action = actionBuffers.DiscreteActions[0];

            switch (action)
            {
                case (int)ActionType.None:
                    break;
                case (int)ActionType.Attack:
                    controllPlayer.Attack(buttleManager.enemy);
                    break;
                case (int)ActionType.Heal:
                    controllPlayer.Heal();
                    break;
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = (int)ActionType.None;

            if (Input.GetKeyDown(KeyCode.A))
            {
                discreteActionsOut[0] = (int)ActionType.Attack;
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                discreteActionsOut[0] = (int)ActionType.Heal;
            }
        }
    }
}

