using UnityEngine;
using UnityEngine.Serialization;

namespace GroveGames.Steering.Unity
{
    [CreateAssetMenu(menuName = "GroveGames/Steering/Context Steering Settings", fileName = "ContextSteeringSettings")]
    public sealed class UnityContextSteeringSettings : ScriptableObject, IContextSteeringSettings
    {
        [Header("Slots")]
        [SerializeField, Range(4, 64), FormerlySerializedAs("_directionCount")] private int _slotCount = 16;

        [Header("Interest")]
        [SerializeField, Range(0f, 1f)] private float _interestSpread = 1f;

        [Header("Danger")]
        [SerializeField, Range(0f, 1f), FormerlySerializedAs("_dangerThreshold")] private float _dangerTolerance = 0.1f;

        [Header("Blending")]
        [SerializeField, Range(0f, 1f)] private float _interestHalfLife = 0.05f;

        public int SlotCount => _slotCount;
        public float InterestSpread => _interestSpread;
        public float DangerTolerance => _dangerTolerance;
        public float InterestHalfLife => _interestHalfLife;
    }
}
