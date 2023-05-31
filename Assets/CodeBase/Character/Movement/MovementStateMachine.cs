using Assets.CodeBase.Character.Movement.MovementStates;
using Assets.CodeBase.Character.Movement.States.Grounded;
using Assets.CodeBase.Infrastructure.Properties;
using Assets.CodeBase.Infrastructure.StateMachine;
using System.Collections.Generic;

namespace Assets.CodeBase.Character.Movement
{
    public class MovementStateMachine: BaseStateMachine<IMovementExitableState>, IUpdatable
    {
        public MovementStateMachine() {
            _states = new Dictionary<System.Type, IMovementExitableState>() {
                [typeof(IdlingState)] = new IdlingState(this),
            };
        }

        public void FixedUpdate() {
            _activeState.FixedUpdate();
        }

        public void HandleInput() {
            _activeState.HandleInput();
        }

        public void Update() {
            _activeState.Update();
        }
    }
}
