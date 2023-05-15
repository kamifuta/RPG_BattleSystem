using InGame.Characters.PlayableCharacters;
using InGame.Damages;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Characters.Enemies
{
    public class EnemyCharacter : BaseCharacter
    {
        private ISubject<PlayableCharacter> attackerSubject = new Subject<PlayableCharacter>();
        public IObservable<PlayableCharacter> AttackerObservable => attackerSubject;

        public EnemyCharacter(CharacterStatus characterStatus) : base(characterStatus)
        {

        }

        public override void ApplyDamage(Damage damage)
        {
            base.ApplyDamage(damage);
            attackerSubject.OnNext(damage.Attacker as PlayableCharacter);
        }
    }
}

