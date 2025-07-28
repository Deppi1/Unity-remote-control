using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

namespace DIVE_Sound
{
    /// <summary>
    /// A class that creates a spatial sound and allows a bit of manipulation with it. Use Play() to create a sound, Pause/Unpause as you probably understand, and Reset to clear the values and stop.
    /// Spatial Sound is intended to use with SoundUtility to pool sounds.
    /// </summary>
    public class SpatialSound : MonoBehaviour
    {
        public UnityEvent<SpatialSound> OnFinishedPlaying = new UnityEvent<SpatialSound>();
        public bool FinishedPlaying
        {
            get
            {
                return _timer >= _length;
            }
        }

        public bool ResetsOnceFinished
        {
            get
            {
                return _resetsOnceFinished;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        private AudioSource _audioSource = null;
        private float _timer = 0.0f;
        private float _length = 0.1f;
        private bool _looping = false;
        private bool _isPlaying = false;
        private bool _resetsOnceFinished = false;
        private float _pitch = 1.0f;

        public void Play(AudioClip clip, float volume, AudioMixerGroup mixerGroup, float spatialBlend = 1f, float randomizePitch = 0,  bool looping = false, bool resetOnceFinished = true)
        {
            _looping = looping;
            _resetsOnceFinished = resetOnceFinished;

            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.enabled = true;
            _audioSource.clip = clip;
            _audioSource.outputAudioMixerGroup = mixerGroup;
            _audioSource.pitch = GetRandomizedPitch(randomizePitch);
            _audioSource.spatialBlend = spatialBlend;
            _audioSource.volume = volume;
            _audioSource.loop = _looping;

            _pitch = _audioSource.pitch;
            _timer = 0.0f;
            _length = clip.length;
            _isPlaying = true;
            _audioSource.Play();
        }

        public void Pause()
        {
            _isPlaying = false;
            _audioSource.Pause();
        }

        public void Unpause()
        {
            _isPlaying = true;
            _audioSource.UnPause();
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _timer += Time.deltaTime;
            if (_timer > _length)
            {
                if (!_looping)
                    ResetSound();
            }
        }

        public void SetPitch(float newPitch)
        {
            _pitch = newPitch;
            _audioSource.pitch = _pitch;
        }

        private float GetRandomizedPitch(float randomAmount)
        {

            if (randomAmount != 0)
            {
                float randomPitch = Random.Range(-randomAmount, randomAmount);
                return Time.timeScale + randomPitch;
            }

            return Time.timeScale;
        }

        public void ResetSound()
        {
            _audioSource?.Stop();
            _audioSource.enabled = false;
            OnFinishedPlaying?.Invoke(this);
        }
    }
}