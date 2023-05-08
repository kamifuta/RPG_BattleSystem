using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using UniRx;
using MyUtil;
using VContainer;

namespace InGame.Fields
{
    public class EncountPresenter : ControllerBase, IStartable
    {
        private EncountView encountView;
        private FieldManager FieldManager;

        [Inject]
        public EncountPresenter(EncountView encountView, FieldManager fieldManager)
        {
            this.encountView = encountView;
            this.FieldManager = fieldManager;
        }

        public void Start()
        {
            encountView.EncountObservable
                .Subscribe(enemyType =>
                {
                    FieldManager.EncountEnemy(enemyType);
                })
                .AddTo(this);
        }
    }
}

