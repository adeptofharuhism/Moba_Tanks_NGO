using Assets.CodeBase.Character.Movement;
using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

namespace Assets.CodeBase.Character
{
    public class Character : NetworkBehaviour
    {
        [SerializeField] private Rigidbody _characterRigidBody;
        [SerializeField] private CharacterMovementData _movementData;
        [SerializeField] private List<CharacterWheelProfile> _wheelProfiles;

        private CharacterMovement _movement;

        public void Construct(IInputService inputService) {
            _movement = new CharacterMovement(inputService, _characterRigidBody, _movementData, _wheelProfiles);
        }

        private void Awake() {
            Construct(AllServices.Container.Single<IInputService>());
        }

        private void Update() {
            if (!IsOwner)
                return;

            UpdateMovement();
        }

        private void FixedUpdate() {
            if (!IsServer)
                return;

            FixedUpdateMovement();
        }

        private void UpdateMovement() {
            _movement.HandleInput();
            _movement.Update();

            KekServerRpc(_movement.GetInput());
        }

        [ServerRpc]
        private void KekServerRpc(Vector2 movementInput) {
            _movement.SetLol(movementInput);
        }

        private void FixedUpdateMovement() {
            _movement.FixedUpdate();
        }

        [SerializeField] private CinemachineVirtualCamera cameraPrefab;
        public void Start() {
            if (IsOwner) {
                StartCoroutine(CameraSpawnDelay());
            }
        }

        System.Collections.IEnumerator CameraSpawnDelay() {
            yield return new WaitForSeconds(6);
            Debug.Log("Im owner");
            CinemachineVirtualCamera virtualCamera = Instantiate(cameraPrefab);
            Debug.Log("Im spawning");
            virtualCamera.LookAt = transform;
            virtualCamera.Follow = transform;
        }
    }
}
