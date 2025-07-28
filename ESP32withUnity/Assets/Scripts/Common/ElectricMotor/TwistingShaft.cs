using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DIVE_E1
{
    public enum PowerState
    {
        NoPower, LowPower, FullPower
    }

    [RequireComponent(typeof(AudioSource))]
    public class TwistingShaft : SteeringWheel
    {
        [Header("Vibrate settings")]
        [SerializeField] private float VibrateFrequency = 0.5f;
        [SerializeField] private float VibrateAmplitude = 0.35f;

        [SerializeField] private float LowPowerAutomaticRotationSpeed = 50.0f;
        [SerializeField] private float FullPowerAutomaticRotationSpeed = 350.0f;

        [SerializeField] private AudioClip LowPowerElectricMotorSFX;
        [SerializeField] private AudioClip FullPowerElectricMotorSFX;

        private AudioSource _audioSource;

        private Grabber _currentGraber = null;
        public Grabber CurrentGraber => _currentGraber;

        public UnityEvent GrabbedRotatingShaft = new UnityEvent();

        private bool PlayerGrabbedRotatingShaft = false;

        private PowerState _electricMotorPowerState;
        public PowerState ElectricMotorPowerState
        {
            get => _electricMotorPowerState;

            set
            {
                if (_electricMotorPowerState == value)
                    return;

                _electricMotorPowerState = value;

                ResetTargetAngle();
               
                ElectricMotorPowerStateChanged();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _audioSource = GetComponent<AudioSource>();
        }

        public float OffsetBetweenAngle()
        {
            return previousTargetAngle - targetAngle;
        }

        public override void OnGrab(Grabber grabber)
        {
            if (ElectricMotorPowerState == PowerState.LowPower || ElectricMotorPowerState == PowerState.FullPower)
            {
                GrabbedRotatingShaft.Invoke();
                PlayerGrabbedRotatingShaft = true;
            }

            base.OnGrab(grabber);

            _currentGraber = grabber;

            StartCoroutine(SimulateDisabledBehaviour());
        }

        public override void OnRelease()
        {
            base.OnRelease();

            _currentGraber = null;
        }

        public void VibrateController(float duration)
        {
            if (_currentGraber == null)
                return;

            //InputBridgeAdjusted.Instance.VibrateController(VibrateFrequency, VibrateAmplitude, duration, _currentGraber.HandSide);
        }

        protected override void Update()
        {
            if (ElectricMotorPowerState == PowerState.NoPower)
            {
                base.Update();
                return;
            }

            targetAngle += GetAutomaticRotationSpeed() * Time.deltaTime;
            ApplyAngleToSteeringWheel(targetAngle);
        }

        private float GetAutomaticRotationSpeed()
        {
            switch (ElectricMotorPowerState)
            {
                case PowerState.LowPower:
                    float speed = PlayerGrabbedRotatingShaft ? LowPowerAutomaticRotationSpeed : 0.0f;
                    return speed;

                case PowerState.FullPower:
                    return FullPowerAutomaticRotationSpeed;

                default:
                    break;
            }

            return 0.0f;
        }

        private void ElectricMotorPowerStateChanged()
        {
            _audioSource.loop = true;

            switch (ElectricMotorPowerState)
            {
                case PowerState.LowPower:
                    _audioSource.clip = LowPowerElectricMotorSFX;
                    _audioSource.Play();
                    break;

                case PowerState.FullPower:
                    _audioSource.clip = FullPowerElectricMotorSFX;
                    _audioSource.Play();
                    break;

                case PowerState.NoPower:
                    _audioSource.Stop();
                    break;

                default:
                    break;
            }
        }

        private IEnumerator SimulateDisabledBehaviour()
        {
            float duration = 0.1f;

            while (_currentGraber != null && enabled == false)
            {
                VibrateController(duration);
                yield return new WaitForSeconds(duration);
            }
        }

        private void ResetTargetAngle()
        {
            targetAngle = 0.0f;
            previousTargetAngle = 0.0f;
            smoothedAngle = 0.0f;
            previousPrimaryPosition = Vector3.zero;
            previousSecondaryPosition = Vector3.zero;
        }
    }
}
