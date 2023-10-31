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

        [SerializeField] private AnimationCurve _frontWheelSteeringTraction;
        [SerializeField] private AnimationCurve _rearWheelSteeringTraction;

        [SerializeField] private float _maxSpeed;
        [SerializeField] private AnimationCurve _enginePower;

        private CarWheel[] _carWheels;

        private void Start() {
            CreateCarWheels();
        }

        private void FixedUpdate() {
            foreach (CarWheel wheel in _carWheels) {
                wheel.ApplyForce();
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
                        _wheelDiameter,
                        (i < 2) ? _frontWheelSteeringTraction : _rearWheelSteeringTraction,
                        _maxSpeed,
                        _enginePower);

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
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * _wheelDiameter);
            }
        }
    }

    public class CarWheel
    {
        private readonly Transform _wheelTransform;
        private readonly LayerMask _carCollisionLayerMask;
        private readonly Rigidbody _carRigidbody;
        private readonly float _springRestDistance;
        private readonly float _springStrength;
        private readonly float _springDamper;
        private readonly float _diameter;
        private readonly AnimationCurve _steeringTraction;
        private readonly float _maxSpeed;
        private readonly AnimationCurve _enginePower;

        private bool _hasChild = false;
        private Transform _wheelChild;

        public CarWheel(
            Transform carLocalTransform, LayerMask carCollisionLayerMask,
            Rigidbody carRigidbody,
            float springRestDistance, float springStrength, float springDamper, float diameter,
            AnimationCurve steeringTraction, 
            float maxSpeed, AnimationCurve enginePower) {

            _wheelTransform = carLocalTransform;
            _carCollisionLayerMask = carCollisionLayerMask;
            _carRigidbody = carRigidbody;
            _springRestDistance = springRestDistance;
            _springStrength = springStrength;
            _springDamper = springDamper;
            _diameter = diameter;
            _steeringTraction = steeringTraction;
            _enginePower = enginePower;
            _maxSpeed = maxSpeed;
        }

        public void TryGetWheelChild() {
            if (_wheelTransform.childCount > 0) {
                _hasChild = true;
                _wheelChild = _wheelTransform.GetChild(0);
            }
        }

        public void ApplyForce() {
            RaycastHit hitInfo;
            bool rayDidHit =
                Physics.Raycast(
                    _wheelTransform.position,
                    -_wheelTransform.up,
                    out hitInfo,
                    _springRestDistance,
                    _carCollisionLayerMask);

            if (rayDidHit) {
                Vector3 springDirectionY = _wheelTransform.up;
                Vector3 steeringDirection = _wheelTransform.right;
                Vector3 accelerationDirection = _wheelTransform.forward;
                Vector3 wheelWorldVelocity = _carRigidbody.GetPointVelocity(_wheelTransform.position);
                //y
                float offsetFromRestY = _springRestDistance - hitInfo.distance;
                float velocityY = Vector3.Dot(springDirectionY, wheelWorldVelocity);
                float forceY = (offsetFromRestY * _springStrength) - (velocityY * _springDamper);
                //x
                float velocity = Vector3.Dot(steeringDirection, wheelWorldVelocity);
                float force = velocity * 1f / Time.fixedDeltaTime;
                //z
                float velocityZ = Vector3.Dot(accelerationDirection, wheelWorldVelocity);
                float normalizedVelocityZ = Mathf.Clamp01(Mathf.Abs(velocityZ) / _maxSpeed);
                float forceZ = _enginePower.Evaluate(normalizedVelocityZ) * 1;

                _carRigidbody.AddForceAtPosition(springDirectionY * forceY, _wheelTransform.position);
                _carRigidbody.AddForceAtPosition(-steeringDirection * force, _wheelTransform.position);
                _carRigidbody.AddForceAtPosition(accelerationDirection * forceZ, _wheelTransform.position);

                SetChildPosition(hitInfo.distance);

                Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (springDirectionY * forceY), Color.green, Time.deltaTime);
                Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (-steeringDirection * force), Color.red, Time.deltaTime);
                Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (accelerationDirection * forceZ), Color.blue, Time.deltaTime);
            } else {
                SetChildPosition(_springRestDistance);
            }
        }

        private void SetChildPosition(float distanceToChild) {
            if (_hasChild)
                _wheelChild.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _diameter);
        }
    }
}
