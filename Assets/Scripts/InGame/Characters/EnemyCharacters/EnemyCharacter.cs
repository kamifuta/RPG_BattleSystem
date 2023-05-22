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
        private ISubject<(PlayableCharacter, int)> attackerSubject = new Subject<(PlayableCharacter, int)>();
        public IObservable<(PlayableCharacter, int)> AttackerObservable => attackerSubject;

        public EnemyCharacter(CharacterStatus characterStatus) : base(characterStatus)
        {

        }

        public override void ApplyDamage(Damage damage)
        {
            base.ApplyDamage(damage);
            attackerSubject.OnNext((damage.attacker as PlayableCharacter, CalcDamage(damage)));
        }
    }
}

