using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using BNG;
using DIVE_Utilities;

namespace DIVE_Sound
{
    /// <summary>
    /// SoundUtility is intended as a solution for pooling audio sources when you need a lot of different ones. Use PlaySpatialClipAt to create a sound from pool.
    /// If you need sound to persist, use resetOnceFinished = false, get returned value and save it and manually call Reset() when needed.
    /// </summary>
    public class SoundUtility : MonoBehaviour
    {
        public AudioMixer MainMixer;
        private PrefabPool<SpatialSound> _soundPool;
        private readonly int _spatialSoundPoolSize = 30;
        private AudioMixerGroup[] _mixerGroups;

        public enum SoundMixerGroups { Master = 0, Effects = 1, Environment = 2, Music = 3, UI = 4, Voice = 5, Snapshot = 6}

        #region SINGLETON
        public static SoundUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundUtility>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("Sound utility").AddComponent<SoundUtility>();
                    }
                }
                return _instance;
            }
        }
        private static SoundUtility _instance;
        #endregion

        private void Start()
        {
            _soundPool = new PrefabPool<SpatialSound>(_spatialSoundPoolSize);
            _mixerGroups = new AudioMixerGroup[6];

            _mixerGroups[0] = MainMixer.FindMatchingGroups("Master")[0];
            _mixerGroups[1] = MainMixer.FindMatchingGroups("Effects")[0];
            _mixerGroups[2] = MainMixer.FindMatchingGroups("Environment")[0];
            _mixerGroups[3] = MainMixer.FindMatchingGroups("Music")[0];
            _mixerGroups[4] = MainMixer.FindMatchingGroups("UI")[0];
            _mixerGroups[5] = MainMixer.FindMatchingGroups("Voice")[0];
        }

        public SpatialSound PlaySpatialClipAt(AudioClip clip, Vector3 pos, float volume, SoundMixerGroups group = SoundMixerGroups.Master, float spatialBlend = 1f, float randomizePitch = 0, bool looping = false, bool resetOnceFinished = true, Transform lockToParent = null)
        {
            if (clip == null)
                return null;

            SpatialSound sound = _soundPool.Get();
            GameObject spatialSoundGameObject;

            if (sound == null)
            {
                spatialSoundGameObject = new GameObject("DIVE spatial sound - " + clip.name);
                sound = spatialSoundGameObject.AddComponent<SpatialSound>();
            }
            else
            {
                spatialSoundGameObject = sound.gameObject;
                spatialSoundGameObject.name = "DIVE spatial sound - " + clip.name;
            }

            if (!lockToParent)
            {
                spatialSoundGameObject.transform.parent = transform;
                spatialSoundGameObject.transform.position = pos;
            }
            else
            {
                spatialSoundGameObject.transform.parent = lockToParent;
                spatialSoundGameObject.transform.localPosition = Vector3.zero;
            }

            sound.enabled = true;
            sound.OnFinishedPlaying.AddListener(OnFinishedPlaying);
            sound.Play(clip, volume, _mixerGroups[(int)group], spatialBlend, randomizePitch, looping);
            return sound;
        }

        private void OnFinishedPlaying(SpatialSound sound)
        {
            sound.OnFinishedPlaying.RemoveListener(OnFinishedPlaying);
            if (sound.ResetsOnceFinished)
            {
                sound.ResetSound();
                sound.enabled = false;
                _soundPool.Add(sound);
            }
        }
    }
}
