using UnityEngine;
using UnityEngine.Events;
using BNG;
using DiveQuestSystem;
using System;
using DIVE_Sound;

namespace DIVE_Common
{
    public class DiveValve: SteeringWheel, IInteractable
    {
        public enum RotationTypeEnum { Clockwise = 0, Counterclockwise = 1 }
        public RotationTypeEnum RotationType;

        public enum ValveState { Opened = 0, Mid = 1, Closed = 2 }
        public ValveState CurrentState 
        {
            get {
                return _currentState;
            }
        }
        public float InitialAngle;

        [Header("Audio")]
        [SerializeField] private AudioClip _rotationSound;
        [SerializeField] private float _rotationSoundVolume = 1.0f;
        private float _rotationSoundInterval = 0.3f;
        private float _rotationTimer = 0.0f;
        //private SpatialSound _sound = null;

        [Header("Events")]
        public UnityEvent<DiveValve> OnOpened;
        public UnityEvent<DiveValve> OnClosed;
        public UnityEvent<DiveValve> OnOpenedToMid;
        public UnityEvent<DiveValve> OnClosedToMid;

        [Header("For display")]
        public float AngleDebug;

        public Action<IInteractableParams> ChangeStateEvent { get => _changeStateEvent; set => _changeStateEvent = value; }

        private ValveState _currentState;
        private bool _blocked = false;
        private Action<IInteractableParams> _changeStateEvent;

        private void Start()
        {
            SetAngle(InitialAngle);
        }

        public override void OnGrab(Grabber grabber)
        {
            base.OnGrab(grabber);
            /*if (_sound != null)
                _sound.ResetSound();

            _sound = SoundUtility.Instance.PlaySpatialClipAt(_rotationSound, transform.position, _rotationSoundVolume, randomizePitch: 0.1f, looping: true, resetOnceFinished: false);
            _sound.Pause();*/
        }


        public override void OnRelease()
        {
            base.OnRelease();
            _rotationTimer = 0.0f;
            /*if (_sound != null)
                _sound.ResetSound();*/
        }

        public void BlockWheel(bool state)
        {
            _blocked = state;
        }

        public void SetAngle(float newAngle)
        {
            // something is wrong here
            targetAngle = newAngle;
            previousTargetAngle = newAngle - 1.0f;
            smoothedAngle = newAngle;
            ApplyAngleToSteeringWheel(newAngle);
            CallEvents();
            UpdatePreviewText();
            UpdatePreviousAngle(newAngle);

            //CheckOpenState();
            //AngleDebug = Angle;
        }

        public void SetState(ValveState newState)
        {
            float targetAngle;
            if (RotationType == RotationTypeEnum.Clockwise)
            {
                switch (newState)
                {
                    case ValveState.Opened:
                        targetAngle = MaxAngle;
                        break;
                    case ValveState.Mid:
                        targetAngle = (MaxAngle + MinAngle) / 2.0f;
                        break;
                    case ValveState.Closed:
                        targetAngle = MinAngle;
                        break;
                    default:
                        targetAngle = MinAngle;
                        break;
                }
            }
            else
            {
                switch (newState)
                {
                    case ValveState.Opened:
                        targetAngle = MinAngle;
                        break;
                    case ValveState.Mid:
                        targetAngle = (MaxAngle + MinAngle) / 2.0f;
                        break;
                    case ValveState.Closed:
                        targetAngle = MaxAngle;
                        break;
                    default:
                        targetAngle = MinAngle;
                        break;
                }
            }

            SetAngle(targetAngle);

            CheckOpenState();
        }

        protected override void Update()
        {
            base.Update();
            // checking the being held requirement
            if (!PrimaryGrabber)
                return;

            /*if (_sound)
            {
                _rotationTimer += Time.deltaTime;
                if (_rotationTimer > _rotationSoundInterval && _sound.IsPlaying)
                {
                    _rotationTimer = 0.0f;
                    _sound.Pause();
                }
            }*/
        }

        public override void UpdateAngleCalculations()
        {
            if (_blocked)
                return;

            base.UpdateAngleCalculations();

            OnRotationSFX();
            CheckOpenState();
            AngleDebug = Angle;
        }

        public override void CallEvents()
        {
            // Call events
            if (targetAngle != previousTargetAngle)
            {
                onAngleChange.Invoke(targetAngle);
                OnRotationSFX();
            }

            onValueChange.Invoke(ScaleValue);
        }

        private void OnRotationSFX()
        {
            /*if (_sound)
            {
                float diff = Mathf.Abs(targetAngle - previousTargetAngle);
                if (!_sound.IsPlaying && diff > 0.01f)
                {
                    _sound.Unpause();
                }
            }*/
        }

        private void CheckOpenState()
        {
            float percentAngle = (targetAngle - MinAngle) / ((MaxAngle - MinAngle) / 100.0f);
            bool openedCondition = RotationType == RotationTypeEnum.Clockwise ? percentAngle > 98.5f : percentAngle < 1.5f;
            bool closedCondition = RotationType == RotationTypeEnum.Clockwise ? percentAngle < 1.5f : percentAngle > 98.5f;

            if (openedCondition && _currentState != ValveState.Opened)
            {
                _currentState = ValveState.Opened;
                OnOpened?.Invoke(this);
                _changeStateEvent?.Invoke(new InteractableParams(_currentState));
                return;
            }

            if (closedCondition && _currentState != ValveState.Closed)
            {
                _currentState = ValveState.Closed;
                OnClosed?.Invoke(this);
                _changeStateEvent?.Invoke(new InteractableParams(_currentState));
                return;
            }

            if (!openedCondition && !closedCondition && _currentState != ValveState.Mid)
            {
                ValveState prevState = _currentState;
                _currentState = ValveState.Mid;

                if (prevState == ValveState.Opened)
                    OnOpenedToMid?.Invoke(this);
                else if (prevState == ValveState.Closed)
                    OnClosedToMid?.Invoke(this);

                _changeStateEvent?.Invoke(new InteractableParams(_currentState));
            }
        }

        public void Execute(object arg)
        {
            InteractableParams valveParam = arg as InteractableParams;

            if (valveParam == null)
            {
                throw new System.ArgumentException("Invalid params type " + arg + " in object " + name);
            }

            SetState(valveParam.State);
        }


        public class InteractableParams : IInteractableParams
        {
            public ValveState State;

            public InteractableParams(ValveState state)
            {
                State = state;
            }

            public bool Equal(IInteractableParams param)
            {
                InteractableParams valveParam = param as InteractableParams;

                return valveParam.State == State;
            }
        }
    }
}
