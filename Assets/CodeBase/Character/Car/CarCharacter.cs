using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.Character.Car
{
    public class CarCharacter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _carRigidBody;
        [SerializeField] private float _wheelRestDistance = 1f;
        [SerializeField] private float _wheelSpringStrength = 50f;
        [SerializeField] private float _wheelSpringDamper = 15f;
        [SerializeField] private float _wheelDiameter = 1f;
        [SerializeField] private LayerMask _carCollisionLayerMask;
        [SerializeField] private List<Transform> _carWheelTransforms;

        private CarWheel[] _carWheels;

        private void Start() {
            CreateCarWheels();
        }

        private void Update() {
            foreach (CarWheel wheel in _carWheels) {
                wheel.ApplySuspensionForce();
            }
        }

        private void CreateCarWheels() {
            _carWheels = new CarWheel[_carWheelTransforms.Count];

            for (int i = 0; i < _carWheelTransforms.Count; i++) {
                _carWheels[i] =
                    new CarWheel(
                        _carWheelTransforms[i],
                        _carCollisionLayerMask,
                        _carRigidBody,
                        _wheelRestDistance,
                        _wheelSpringStrength,
                        _wheelSpringDamper,
                        _wheelDiameter);

                _carWheels[i].TryGetWheelChild();
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.white;
            foreach (Transform transform in _carWheelTransforms) {
                Gizmos.DrawLine(transform.position, transform.position - transform.up * _wheelRestDistance);
            }

            Gizmos.color = Color.cyan;
            foreach (Transform transform in _carWheelTransforms) {
                Gizmos.DrawLine(transform.position, transform.position + transform.up * _wheelDiameter);
            }
        }
    }

    public class CarWheel
    {
        private readonly Transform _wheelTransform;
        private readonly float _wheelRestDistance;
        private readonly LayerMask _carCollisionLayerMask;
        private readonly Rigidbody _carRigidbody;
        private readonly float _wheelSpringStrength;
        private readonly float _wheelSpringDamper;
        private readonly float _wheelDiameter;

        private bool _hasChild = false;
        private Transform _wheelChild;

        public CarWheel(
            Transform carLocalTransform, LayerMask carCollisionLayerMask,
            Rigidbody carRigidbody,
            float wheeRestDistance, float wheelSpringStrength, float wheelSpringDamper, float wheelDiameter) {

            _wheelTransform = carLocalTransform;
            _wheelRestDistance = wheeRestDistance;
            _carCollisionLayerMask = carCollisionLayerMask;
            _carRigidbody = carRigidbody;
            _wheelSpringStrength = wheelSpringStrength;
            _wheelSpringDamper = wheelSpringDamper;
            _wheelDiameter = wheelDiameter;
        }

        public void TryGetWheelChild() {
            if (_wheelTransform.childCount > 0) {
                _hasChild = true;
                _wheelChild = _wheelTransform.GetChild(0);
            }
        }

        public void ApplySuspensionForce() {
            RaycastHit hitInfo;
            bool rayDidHit =
                Physics.Raycast(
                    _wheelTransform.position,
                    -_wheelTransform.up,
                    out hitInfo,
                    _wheelRestDistance,
                    _carCollisionLayerMask);

            if (rayDidHit) {
                Vector3 springDirection = _wheelTransform.up;

                Vector3 wheelWorldVelocity = _carRigidbody.GetPointVelocity(_wheelTransform.position);

                float offsetFromRest = _wheelRestDistance - hitInfo.distance;

                float velocity = Vector3.Dot(springDirection, wheelWorldVelocity);

                float force = (offsetFromRest * _wheelSpringStrength) - (velocity * _wheelSpringDamper);

                _carRigidbody.AddForceAtPosition(springDirection * force, _wheelTransform.position);

                SetChildPosition(hitInfo.distance);
            } else {
                SetChildPosition(_wheelRestDistance);
            }
        }

        private void SetChildPosition(float distanceToChild) {
            if (_hasChild)
                _wheelChild.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _wheelDiameter);
        }
    }
}
