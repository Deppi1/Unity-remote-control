using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;
using DIVE_Utilities;

namespace DIVE_Common
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent (typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class SimpleButton : MonoBehaviour
    {
        public bool IsActive = false;
        public bool TriggerableByUITrigger = true;
        public bool TriggerableByGrabber = false;
        public float ClickTimeout = 1.0f;
        public AudioClip ButtonClick;
        public AudioClip ButtonClickUp;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        [Header("Haptics")]
        public float VibrateFrequency = 0.3f;
        public float VibrateAmplitude = 0.1f;
        public float VibrateDuration = 0.1f;

        private List<Grabber> _grabbers;
        private List<UITriggerAdjusted> _uiTriggers;
        private AudioSource _audioSource;
        private float _clickTimer = 0.0f;
        private bool _timeoutOn = false;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _grabbers = new List<Grabber>();
            _uiTriggers = new List<UITriggerAdjusted>();
        }

        private void Update()
        {
            if (_timeoutOn)
            {
                _clickTimer -= Time.deltaTime;
                if (_clickTimer < 0.0f)
                    _timeoutOn = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_timeoutOn)
                return;

            bool triggered = false;
            ControllerHand handSide = ControllerHand.None;
            // Check Grabber
            if (TriggerableByGrabber)
            {
                Grabber grab = other.GetComponent<Grabber>();
                if (!grab)
                {
                    if (_grabbers == null)
                        _grabbers = new List<Grabber>();

                    if (!_grabbers.Contains(grab))
                    {
                        _grabbers.Add(grab);
                        triggered = true;
                        handSide = grab.HandSide;
                    }
                }
            }

            // Check UITrigger, which is another type of activator
            if (TriggerableByUITrigger)
            {
                UITriggerAdjusted trigger = other.GetComponent<UITriggerAdjusted>();
                if (trigger)
                {
                    if (_uiTriggers == null)
                        _uiTriggers = new List<UITriggerAdjusted>();

                    if (!_uiTriggers.Contains(trigger))
                    {
                        _uiTriggers.Add(trigger);
                        triggered = true;
                        handSide = trigger.ParentGrabber.HandSide;
                    }
                }
            }

            // run the event if button was triggered
            if (triggered)
            {
                DoHaptics(handSide);
                OnButtonDown();
            }
        }

        private void DoHaptics(ControllerHand handSide)
        {
            if (handSide == ControllerHand.None)
                return;

            //GameSceneManager.Instance.Bridge.VibrateController(VibrateFrequency, VibrateAmplitude, VibrateDuration, handSide);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_timeoutOn)
                return;

            bool triggered = false;
            // Check Grabber
            if (TriggerableByGrabber)
            {
                Grabber grab = other.GetComponent<Grabber>();
                if (!grab)
                {
                    if (_grabbers == null)
                        _grabbers = new List<Grabber>();

                    if (_grabbers.Contains(grab))
                    {
                        _grabbers.Remove(grab);
                        triggered = true;
                    }
                }
            }

            // Check UITrigger, which is another type of activator
            if (TriggerableByUITrigger)
            {
                UITriggerAdjusted trigger = other.GetComponent<UITriggerAdjusted>();
                if (trigger)
                {
                    if (_uiTriggers == null)
                        _uiTriggers = new List<UITriggerAdjusted>();

                    if (_uiTriggers.Contains(trigger))
                    {
                        _uiTriggers.Remove(trigger);
                        triggered = true;
                    }
                }
            }

            // run the event if button was triggered
            if (triggered)
            {
                OnButtonUp();
            }
        }

        protected virtual void OnButtonDown()
        {
            IsActive = !IsActive;

            // Play sound
            if (_audioSource && ButtonClick)
            {
                _audioSource.clip = ButtonClick;
                _audioSource.Play();
            }

            // Call event
            if (OnEnter != null)
            {
                OnEnter.Invoke();
            }
        }

        protected virtual void OnButtonUp()
        {
            // Enable timeout
            _timeoutOn = true;
            _clickTimer = ClickTimeout;

            // Play sound
            if (_audioSource && ButtonClick)
            {
                _audioSource.clip = ButtonClickUp;
                _audioSource.Play();
            }

            // Call event
            if (OnExit != null)
            {
                OnExit.Invoke();
            }
        }
    }

}
