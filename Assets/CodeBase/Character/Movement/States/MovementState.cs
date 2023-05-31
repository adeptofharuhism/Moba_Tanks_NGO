using Assets.CodeBase.Character.Movement.MovementStates;

namespace Assets.CodeBase.Character.Movement.States
{
    public class MovementState : IMovementState
    {
        protected MovementStateMachine _stateMachine;

        public MovementState(MovementStateMachine stateMachine) {
            _stateMachine = stateMachine;
        }

        public void Enter() {

        }

        public void Exit() {

        }

        public void HandleInput() {

        }

        public void Update() {

        }

        public void FixedUpdate() {

        }
    }
}
