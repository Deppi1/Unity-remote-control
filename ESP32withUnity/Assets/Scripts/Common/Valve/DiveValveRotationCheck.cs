using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using DiveQuestSystem;
using static UnityEngine.CullingGroup;
using UnityEngine.Events;

namespace DIVE_Common
{
    [RequireComponent(typeof(DiveValve))]
    [RequireComponent(typeof(GrabbableHaptics))]
    [RequireComponent(typeof(GrabbableUnityEvents))]
    public class DiveValveRotationCheck : MonoBehaviour
    {
        public UnityEvent OnGrabCountError;
        [SerializeField] private int _requiredGrabCount;
        [SerializeField] private float _instantErrorThrehold = 2.0f;

        [Tooltip("Haptics")]
        [SerializeField] private float _hapticsFreqWarning;
        [SerializeField] private float _hapticsAmpWarning;
        [SerializeField] private float _hapticsFreqError;
        [SerializeField] private float _hapticsAmpError;

        private DiveValve _valve;
        private GrabbableUnityEvents _grabbableEvents;
        private Grabber _grabber;

        private bool _errorWasActive = false;
        private float _rotationCounter = 0.0f;
        private float _lastAngle = 0.0f;
        private float _maxRotation;

        public bool ShowDebug = false;

        private void Awake()
        {
            _valve = GetComponent<DiveValve>();
            _grabbableEvents = GetComponent<GrabbableUnityEvents>();
        }

        private void Start()
        {
            _maxRotation = (_valve.MaxAngle - _valve.MinAngle) / _requiredGrabCount;
        }

        private void OnEnable()
        {
            _valve.onAngleChange.AddListener(OnAngleChange);
            _grabbableEvents.onGrab.AddListener(OnGrab);
            _grabbableEvents.onRelease.AddListener(OnRelease);
        }

        private void OnDisable()
        {
            _valve.onAngleChange.RemoveListener(OnAngleChange);
            _grabbableEvents.onGrab.RemoveListener(OnGrab);
            _grabbableEvents.onRelease.RemoveListener(OnRelease);
        }

        private void OnAngleChange(float newAngle)
        {
            if (_requiredGrabCount > 0)
            {
                CheckRotationCounter(newAngle);
            }
            _lastAngle = newAngle;
        }

        public void OnGrab(Grabber grabber)
        {
            _grabber = grabber;
            _errorWasActive = false;

            _rotationCounter = 0.0f;
            _lastAngle = _valve.Angle;
        }

        public void OnRelease()
        {
            _grabber = null;
            _errorWasActive = false;

            _rotationCounter = 0.0f;
        }

        private void CheckRotationCounter(float newAngle)
        {
            _rotationCounter += Mathf.Abs(newAngle - _lastAngle);
            if (ShowDebug)
                Debug.Log("rotation counter = " + _rotationCounter);

            if (_rotationCounter > _maxRotation * _instantErrorThrehold)
            {
                if (!_errorWasActive)
                {
                    _errorWasActive = true;
                    OnGrabCountError?.Invoke();
                }

                DoHaptics(_hapticsFreqError, _hapticsAmpError);
                return;
            }
            if (_rotationCounter > _maxRotation)
            {
                DoHaptics(_hapticsFreqWarning, _hapticsAmpWarning);
            }
        }

        protected virtual void DoHaptics(float hapticsFreq, float hapticAmp)
        {
            if (!_grabber)
                return;

            var inputBridge = InputBridge.Instance;

            if (!inputBridge)
                return;

            inputBridge.VibrateController(hapticsFreq, hapticAmp, 0.2f, _grabber.HandSide);
        }
    }
}
