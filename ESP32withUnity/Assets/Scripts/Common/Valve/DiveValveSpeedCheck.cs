using BNG;
using UnityEngine;
using UnityEngine.Events;

namespace DIVE_Common
{
    [RequireComponent(typeof(DiveValve))]
    [RequireComponent(typeof(GrabbableHaptics))]
    [RequireComponent(typeof(GrabbableUnityEvents))]
    public class DiveValveSpeedCheck : MonoBehaviour
    {
        [Tooltip("Degrees per second, exceeding itfor a duration of time will cause warning and then error")]
        public float ErrorRotationSpeed;
        public UnityEvent OnSpeedRequirementError;

        [Tooltip("Time to error")]
        [SerializeField] private float _speedRequirementThresholdTime;
        [SerializeField] private float _instantErrorThreshold = 5.0f;
        [SerializeField] private float _hapticsFreqWarning;
        [SerializeField] private float _hapticsAmpWarning;
        [SerializeField] private float _hapticsFreqError;
        [SerializeField] private float _hapticsAmpError;

        private DiveValve _valve;
        private const float _timeBetweenUpdates = 0.1f;
        private float _timeToNextCheck = 0;
        private float _angleChangeForPeriod = 0;
        private float _lastAngle = 0.0f;
        private bool _countDownIsActive = false;
        private float _timeToError = 0.0f;
        private bool _timeToErrorIsOver = false;
        private bool _errorWasActive = false;

        private GrabbableUnityEvents _grabbableEvents;
        private Grabber _grabber;
        private bool _hapticsEnabled = true;

        public bool ShowDebug = false;

        private void Awake()
        {
            _valve = GetComponent<DiveValve>();
            _grabbableEvents = GetComponent<GrabbableUnityEvents>();
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

        private void Update()
        {
            if (!_grabber)
                return;

            if (_timeToNextCheck <= Time.time)
            {
                _timeToNextCheck = Time.time + _timeBetweenUpdates;
                if (!CheckRotationSpeed())
                {
                    if (ShowDebug)
                        Debug.Log("Rotation speed check failed! time to error over = " + _timeToErrorIsOver);
                    if (!_timeToErrorIsOver)
                        WarningRotation();
                    else
                    {
                        if (ShowDebug)
                            Debug.Log("Time to error is over!");
                        ErrorRotation();
                    }
                        
                }
                _angleChangeForPeriod = 0;
            }
        }

        public void EnableHaptics(bool enable)
        {
            _hapticsEnabled = enable;
        }

        private void OnAngleChange(float newAngle)
        {
            _angleChangeForPeriod += Mathf.Abs(newAngle - _lastAngle);
            _lastAngle = newAngle;
        }

        public void OnGrab(Grabber grabber)
        {
            _grabber = grabber;
            _errorWasActive = false;
            _angleChangeForPeriod = 0.0f;
            _lastAngle = _valve.Angle;
        }

        public void OnRelease()
        {
            _grabber = null;
            _errorWasActive = false;
        }

        private void WarningRotation()
        {
            if (ShowDebug)
                Debug.Log("checking error overflow, " + ErrorRotationSpeed * _instantErrorThreshold + " vs " + _angleChangeForPeriod);
            if (ErrorRotationSpeed * _instantErrorThreshold * _timeBetweenUpdates <= _angleChangeForPeriod)
            {
                if (ShowDebug)
                    Debug.Log("Instant error!");
                ErrorRotation();
                return;
            }
            if (!_countDownIsActive)
            {
                _countDownIsActive = true;
                _timeToError = Time.time + _speedRequirementThresholdTime;
            }
            if (_hapticsEnabled)
                DoHaptics(_hapticsFreqWarning, _hapticsAmpWarning);

            if (_timeToError <= Time.time)
            {
                _timeToErrorIsOver = true;
            }
        }

        private void ErrorRotation()
        {
            if (_errorWasActive) 
                return;

            _errorWasActive = true;
            OnSpeedRequirementError?.Invoke();
            if (_hapticsEnabled)
                DoHaptics(_hapticsFreqError, _hapticsAmpError);
        }

        private bool CheckRotationSpeed()
        {
            if (ShowDebug)
                Debug.Log("checking rotation speed! angle change for period = " + _angleChangeForPeriod);
            if (_angleChangeForPeriod > ErrorRotationSpeed * _timeBetweenUpdates) 
                return false;

            _countDownIsActive = false;
            _timeToErrorIsOver = false;
            return true;
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
