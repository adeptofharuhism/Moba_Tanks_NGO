using Assets.CodeBase.Character.Movement;
using Assets.CodeBase.Character.Movement.States.Grounded;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CodeBase.Character
{
    public class Character: MonoBehaviour
    {
        private MovementStateMachine _movementStateMachine;

        private void Awake() {
            _movementStateMachine = new MovementStateMachine();
            _movementStateMachine.Enter<IdlingState>();
        }
    }
}
