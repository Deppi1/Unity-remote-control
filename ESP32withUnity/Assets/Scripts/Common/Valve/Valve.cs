using UnityEngine;
using UnityEngine.Events;
using BNG;
using DiveQuestSystem;
using System;
using DIVE_Utilities;
using DIVE_Sound;

namespace DIVE_Common
{
    [RequireComponent(typeof(SteeringWheelAdjusted))]
    [RequireComponent(typeof(GrabbableHaptics))]
    public class Valve : GrabbableEvents, IInteractable
    {
        public UnityEvent OnGrabCountError;
        public UnityEvent OnSpeedRequirementError;
        public UnityEvent<bool, Valve> ValveStateChanged;
        [Header("Errors")]

        [SerializeField]
        private float _hapticsFreqWarning;
        [SerializeField]
        private float _hapticsAmpWarning;
        [SerializeField]
        private float _hapticsFreqError;
        [SerializeField]
        private float _hapticsAmpError;
        

        [Header("RotationSettings")]

        [Tooltip("Enable rotation speed tracking")]
        [SerializeField]
        private bool _speedRequirement;
        [Tooltip("Degrees per second, exceeding itfor a duration of time will cause warning and then error")]
        public float ErrorRotationSpeed;
        [Tooltip("Time to error")]
        [SerializeField]
        private float _speedRequirementThresholdTime;
        [Tooltip("The number of parts into which the rotation will be divided")]
        [SerializeField]
        private bool _grabsRequirement;
        [SerializeField]
        private int _requiredGrabCount;
        [SerializeField]
        private float _maxRotatePercent;
        private float _rotatePercentCounter = 0;
        private float _maxTurnPerSec = 0; // how many percent can you turn in a second
        private float _timeToError = 0;
        private int _rotateCounter = 0;
        private float _timeToNextCheck = 0;
        private float _percentBetweenChecks = 0;
        private const float _timeBetweenUpdates = 0.1f;
        private bool _countDownIsActive = false;
        [SerializeField]
        private bool _valveOpen;
        private SteeringWheelAdjusted _steeringWheelScr;
        private GrabbableHaptics _gh;
        private float _lastUpdatePercentOfValve;
        private float _percent;
        private bool _timeToErrorIsOver = false;
        private bool _hapticsIsZero = false;
        private bool _errorWasActive = false;

        [Header("Audio")]
        [SerializeField] private AudioClip _rotationSound;
        [SerializeField] private float _rotationSoundVolume = 1.0f;
        private float _rotationTimer = 0.0f;
        private float _rotationInterval = 0.3f;
        private SpatialSound _sound = null;

        public Action<IInteractableParams> OnStateChanged;

        public bool Open {
            get 
            {
                return _valveOpen;
            }
        }

        public SteeringWheelAdjusted BaseWheel {
            get {
                return _steeringWheelScr;
            }
        }
        
        new private void Awake()
        {
            base.Awake();
            _steeringWheelScr = GetComponent<SteeringWheelAdjusted>();
            _gh = GetComponent<GrabbableHaptics>();
        }

        private void Start() {
            _lastUpdatePercentOfValve = _steeringWheelScr.ScaleValue;

            if (_hapticsFreqWarning == 0 || _hapticsAmpWarning == 0 || _hapticsFreqError == 0 || _hapticsAmpError == 0)
            {
                _hapticsIsZero = true;
            }
            if (_grabsRequirement && _requiredGrabCount > 0)
            {
                _maxRotatePercent = ((_steeringWheelScr.MaxAngle - _steeringWheelScr.MinAngle) * 100) / ((_steeringWheelScr.MaxAngle - _steeringWheelScr.MinAngle) * _requiredGrabCount);
            }
            if (_speedRequirement)
            {
                _maxTurnPerSec = ErrorRotationSpeed / ((_steeringWheelScr.MaxAngle - _steeringWheelScr.MinAngle) / 100);
            }
            _rotatePercentCounter = 0;
        }

        private void OnEnable()
        {
            _steeringWheelScr.onAngleChange.AddListener(UpdatePercent);
        }

        private void OnDisable()
        {
            _steeringWheelScr.onAngleChange.RemoveListener(UpdatePercent);
        }


        public override void OnGrab(Grabber grabber)
        {
            _rotatePercentCounter = 0;
            _errorWasActive = false;
            
            base.OnGrab(grabber);
            //if (_sound != null)
                //_sound.ResetSound();

            //_sound = SoundUtility.Instance.PlaySpatialClipAt(_rotationSound, transform.position, _rotationSoundVolume, randomizePitch: 0.3f, looping: true, resetOnceFinished: false);
            //_sound.Pause();
        }


        public override void OnRelease()
        {
            base.OnRelease();
            if (_sound != null)
                _sound.ResetSound();

            _rotatePercentCounter = 0;
            _errorWasActive = false;
        }


        /// <summary>
        /// Returns the rotation percentage
        /// </summary>
        public float Percent
        {
            get
            {
                return _percent;
            }
        }


        private bool RotationSpeedIsNormal()
        {
            if (_maxTurnPerSec * _timeBetweenUpdates < _percentBetweenChecks * _timeBetweenUpdates) return false;
            _countDownIsActive = false;
            _timeToErrorIsOver = false;
            return true;
        }


        private void CheckRotationCounter(float rotationPercent)
        {
            _rotatePercentCounter += Mathf.Abs(rotationPercent - _lastUpdatePercentOfValve);
            if (_rotatePercentCounter > _maxRotatePercent * 2)
            {
                if (!_errorWasActive)
                {
                    _errorWasActive = true;
                    OnGrabCountError?.Invoke();
                    _timeToErrorIsOver = true;
                }

                DoHaptic(_hapticsFreqError, _hapticsAmpError);
                return;
            }
            if (_rotatePercentCounter > _maxRotatePercent)
            {
                DoHaptic(_hapticsFreqWarning, _hapticsAmpWarning);
            }
        }
        

        private void WarningRotation()
        {
            if (_maxTurnPerSec * _timeBetweenUpdates * 3 <= _percentBetweenChecks * _timeBetweenUpdates)
            {
                ErrorRotation();
                return;
            }
            if (!_countDownIsActive)
            {
                _countDownIsActive = true;
                _timeToError = Time.time + _speedRequirementThresholdTime;
            }
            DoHaptic(_hapticsFreqWarning, _hapticsAmpWarning);
            if (_timeToError <= Time.time)
            {
                _timeToErrorIsOver = true;
            }
        }
        
        
        private void ErrorRotation()
        {
            if (_errorWasActive) return;
            
            _errorWasActive = true;
            OnSpeedRequirementError?.Invoke();
            DoHaptic(_hapticsFreqError, _hapticsAmpError);
        }


        private void Update() 
        {
            // checking the being held requirement
            if (_steeringWheelScr.PrimaryGrabber == null)
                return;

            if (_sound)
            {
                _rotationTimer += Time.deltaTime;
                if (_rotationTimer > _rotationInterval && _sound.IsPlaying)
                {
                    _sound.Pause();
                }
            }

            // checking the speed requirement
            if (!_speedRequirement) return;
            
            if (_timeToNextCheck <= Time.time)
            {
                _timeToNextCheck = Time.time + _timeBetweenUpdates;
                if (!RotationSpeedIsNormal())
                {
                    if (!_timeToErrorIsOver)
                    {
                       WarningRotation();
                    }
                    else
                    {
                        ErrorRotation();
                    }
                }
                if (_percentBetweenChecks != 0)
                {
                    _rotateCounter++;
                }
                _percentBetweenChecks = 0;
            }
        }


        /// <summary>
        /// Updates valve status values and check smooth rotation
        /// </summary>
        public void UpdatePercent(float angle)
        {
            float value = (angle - _steeringWheelScr.MinAngle) / ((_steeringWheelScr.MaxAngle - _steeringWheelScr.MinAngle) / 100) ;

            if (Mathf.Abs(_lastUpdatePercentOfValve - value) < 0.005f)
            {
                if (_sound)
                    _sound.Pause();
            }
            else if (_sound)
            {
                _sound.Unpause();
                _rotationTimer = 0.0f;
            }
                

            _percentBetweenChecks += Mathf.Abs(value - _lastUpdatePercentOfValve)*50;
            if (_grabsRequirement && _requiredGrabCount > 0)
            {
                CheckRotationCounter(value);
            }
            
            _lastUpdatePercentOfValve = value;
            _percent = value;
            
            if (_percent > 99 && _valveOpen == false)
            {
                _valveOpen = true;
                ValveStateChanged.Invoke(_valveOpen, this);
                OnStateChanged?.Invoke(new InteractableBoolParam(_valveOpen));
            }
            if (_percent < 1 && _valveOpen == true)
            {
                _valveOpen = false;
                ValveStateChanged.Invoke(_valveOpen, this);
                OnStateChanged?.Invoke(new InteractableBoolParam(_valveOpen));
            }
        }


        public void SetWheelBlockedState(bool enable)
        {
            if (_steeringWheelScr == null)
            {
                _steeringWheelScr = GetComponent<SteeringWheelAdjusted>();
            }
            if (_steeringWheelScr != null)
            {
                _steeringWheelScr.enabled = enable;
            }
        }


        protected virtual void DoHaptic(float hapticsFreq, float hapticAmp)
        {
            if (_hapticsIsZero || thisGrabber == null) return;

            var inputBridge = InputBridge.Instance;
            if (inputBridge == null) return;

            inputBridge.VibrateController(hapticsFreq, hapticAmp, 0.2f, thisGrabber.HandSide);
        }


        public void Execute(object param)
        {
            InteractableBoolParam valveParam = param as InteractableBoolParam;
            if (valveParam == null)
            {
                throw new System.ArgumentException("Invalid params type " + param + " in object " + name);
            }

            if (valveParam.Param)
            {
                BaseWheel.SetOpened();
                return;
            }
            BaseWheel.SetClosed();
        }


        public Action<IInteractableParams> ChangeStateEvent
        {
            get
            {
                return OnStateChanged;
            }
            set
            {
                OnStateChanged = value;
            }
        }


        public IInteractableParams GetParams()
        {
            return new InteractableBoolParam(_valveOpen);
        }
    }
}
