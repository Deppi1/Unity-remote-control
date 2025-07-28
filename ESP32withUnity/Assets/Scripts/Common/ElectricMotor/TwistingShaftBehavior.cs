using System.Collections;
using UnityEngine;

namespace DIVE_E1
{
    public enum TwistingShaftType
    {
        Disable,
        Working,
        Broken
    }

    public class TwistingShaftBehavior : MonoBehaviour
    {
        [SerializeField] private TwistingShaftType TwistingShaftType;

        private TwistingShaft _twistingShaft;

        private int _angleChangedCallCount = 0;
        private int _maxAngleChangedCallCount = 75;

        [SerializeField] private float MaxAngleOffset = 120.0f;
        private float _currentOffsetBetwenAngle = 0.0f;

        private void Awake()
        {
            _twistingShaft = GetComponent<TwistingShaft>();

            SetTwistingShaftType(TwistingShaftType);
        }

        private void OnEnable()
        {
            _twistingShaft.onAngleChange.AddListener(SteeringWheelAngleChanged);
        }

        private void OnDisable()
        {
            _twistingShaft.onAngleChange.RemoveListener(SteeringWheelAngleChanged);
        }

        private void SteeringWheelAngleChanged(float angle)
        {
            _currentOffsetBetwenAngle += _twistingShaft.OffsetBetweenAngle();

            if (Mathf.Abs(_currentOffsetBetwenAngle) > MaxAngleOffset)
            {
                _currentOffsetBetwenAngle = 0.0f;
                _twistingShaft.CurrentGraber?.TryRelease();
                return;
            }

            SimulateBrokenBehavior();
        }

        private void SimulateBrokenBehavior()
        {
            if (TwistingShaftType != TwistingShaftType.Broken)
                return;

            if (_angleChangedCallCount > _maxAngleChangedCallCount)
            {
                _angleChangedCallCount = 0;

                _maxAngleChangedCallCount = UnityEngine.Random.Range(50, 100);

                float delaySec = UnityEngine.Random.Range(0.1f, 0.5f);

                _twistingShaft.VibrateController(delaySec);

                StartCoroutine(DelayTwisting(delaySec));
            }
            else
            {
                _angleChangedCallCount++;
            }
        }

        private IEnumerator DelayTwisting(float delaySec)
        {
            _twistingShaft.enabled = false;

            yield return new WaitForSeconds(delaySec);

            _twistingShaft.enabled = true;
        }

        public void SetTwistingShaftType(TwistingShaftType type)
        {
            TwistingShaftType = type;

            switch (TwistingShaftType)
            {
                case TwistingShaftType.Disable:
                    _twistingShaft.enabled = false;
                    break;
                case TwistingShaftType.Working:
                case TwistingShaftType.Broken:
                    _twistingShaft.enabled = true;
                    break;
            }
        }

    }
}