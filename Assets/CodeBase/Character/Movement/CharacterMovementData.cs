using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [Serializable]
    public class CharacterMovementData
    {
        [SerializeField] private float _speed = 5f;

        public float Speed => _speed;
    }
}
